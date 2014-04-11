using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Tasks
{
    public class TaskExecutor : IDisposable
    {
        private ConcurrentQueue<Task> _TaskQueue;
        private AutoResetEvent _AreTaskQueue;
        private AutoResetEvent _AreErrorAndResultQueue;
        private AutoResetEvent _AreTaskQueueFinished;
        private AutoResetEvent _AreErrorAndResultQueueFinished;
        private ConcurrentQueue<TaskResults> _ResultQueue;
        private ConcurrentQueue<TaskError> _ErrorQueue;
        private ReaderWriterLockSlim _ListsLock;
        private List<Thread> _WorkerThreads;
        private List<long> _ErroredTasks;
        private List<long> _ExecutedTasks;
        private Thread _ErrorAndResultQueueThread;
        public bool Disposed
        {
            get;
            private set;
        }
        public void Dispose()
        {
            Disposed = true;
            _AreTaskQueue.Set();
            _AreErrorAndResultQueue.Set();
            _AreTaskQueueFinished.WaitOne();
            _AreErrorAndResultQueueFinished.WaitOne();
            foreach (Thread t in _WorkerThreads)
            {
                t.Abort();
            }
           
        }
        public TaskExecutor(int workercount)
        {
            Disposed = false;
            _AreTaskQueueFinished = new AutoResetEvent(false);
            _AreErrorAndResultQueueFinished = new AutoResetEvent(false);
            _ListsLock = new ReaderWriterLockSlim();
            _ExecutedTasks = new List<long>();
            _ErroredTasks = new List<long>();
            _WorkerThreads = new List<Thread>();
            _ResultQueue = new ConcurrentQueue<TaskResults>();
            _ErrorQueue = new ConcurrentQueue<TaskError>();
            _AreTaskQueue = new AutoResetEvent(false);
            _AreErrorAndResultQueue = new AutoResetEvent(false);
            _TaskQueue = new ConcurrentQueue<Task>();
            for (int i = 0; i < workercount; i++)
            {
                
                Thread t = new Thread(new ThreadStart(_CheckQueue));
                _WorkerThreads.Add(t);
                t.Start();
            }
            _ErrorAndResultQueueThread = new Thread(new ThreadStart(_CheckErrorsAndResults));
            _ErrorAndResultQueueThread.Start();
        }
        private void _CheckErrorsAndResults()
        {
            while (!Disposed)
            {
                _AreErrorAndResultQueue.WaitOne();
                TaskError te = null;
                if (_ErrorQueue.TryDequeue(out te))
                {
                    te.ErroredTask.TriggerTaskErrored(new TaskErroredEventArgs(te));
                    _AreErrorAndResultQueue.Set();
                }
                TaskResults tr = null;
                if (_ResultQueue.TryDequeue(out tr))
                {
                    tr.CompletedTask.TriggerTaskCompleted(new TaskCompletedEventArgs(tr));
                    _AreErrorAndResultQueue.Set();
                }
            }
            _AreErrorAndResultQueueFinished.Set();
        }
        public void SubmitTask(Task task)
        {
            _TaskQueue.Enqueue(task);
            _AreTaskQueue.Set();
        }
        private void _CheckQueue()
        {
            while (!Disposed)
            {
                _AreTaskQueue.WaitOne();
                Task t = null;
                if (_TaskQueue.TryDequeue(out t))
                {
                    TaskResults res = null;
                    TaskError err = null;
                    _ListsLock.EnterReadLock();
                    bool prereqok = true;
                    bool prereqerrorok = true;
                    long prereqerrorid = 0;
                    foreach (long tid in t.PreReqTasks)
                    {
                        if (!_ExecutedTasks.Contains(tid))
                        {
                            prereqok = false;
                        }
                        if (_ErroredTasks.Contains(tid))
                        {
                            prereqerrorok = false;
                            prereqerrorid = tid;
                        }
                    }
                    _ListsLock.ExitReadLock();
                    //If not all the prereqs have been met
                    //but no errors in the prereqs have occured.
                    if (!prereqok && prereqerrorok)
                    {
                        _TaskQueue.Enqueue(t);
                        _AreTaskQueue.Set();
                        //Just go to the top of the loop and wait.
                        continue;
                    }
                    //If not all one or more of the prereqs have errored.
                    if (!prereqerrorok)
                    {
                        _ListsLock.EnterWriteLock();
                        _ErroredTasks.Add(t.ID);
                        _ListsLock.ExitWriteLock();
                        _ErrorQueue.Enqueue(new PreReqTaskError(t, prereqerrorid));
                        _AreErrorAndResultQueue.Set();
                        //Just go to the top of the loop and wait.
                        continue;
                    }
                    t.Execute(out res, out err);
                    //Need to update the lists with the newly executed task.
                    _ListsLock.EnterWriteLock();
                    _ExecutedTasks.Add(t.ID);
                    if (err != null)
                    {
                        _ErroredTasks.Add(t.ID);
                        _ErrorQueue.Enqueue(err);
                        _AreErrorAndResultQueue.Set();
                    }
                    _ListsLock.ExitWriteLock();
                    if (res != null)
                    {
                        _ResultQueue.Enqueue(res);
                        _AreErrorAndResultQueue.Set();
                    }
                    _AreTaskQueue.Set();
                }
            }
            _AreTaskQueueFinished.Set();
        }

    }
}
