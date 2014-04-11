using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace Tasks
{
    public enum TaskType
    {
        UpdateFeed,
        DownloadPage,
        SaveFeedState,
        ComputeWordCount,
        GetMarketInformation,
        TaskBlock,
    }
    public abstract class Task
    {
        /// <summary>
        /// This gets called when TrigerTaskCompleted is called
        /// This is done automatically by task executor.
        /// </summary>
        public event EventHandler<TaskCompletedEventArgs> TaskCompleted;
        /// <summary>
        /// This gets called when TriggerTaskErrored is called.
        /// This is done automatically by task executor.
        /// </summary>
        public event EventHandler<TaskErroredEventArgs> TaskErrored;
        /// <summary>
        /// The current ID of the task. This is static because ids are meant
        /// to be globaly unique.
        /// </summary>
        private static long _CurrentID = 0;
        /// <summary>
        /// Gets the next ID for use.
        /// </summary>
        /// <returns>The next ID for use.</returns>
        private static long _GetNextID()
        {
            return Interlocked.Increment(ref _CurrentID);
        }
        /// <summary>
        /// A list of tasks that must be executed before this one.
        /// </summary>
        private List<long> _PreReqTasks;
        /// <summary>
        /// The ID of the task.
        /// </summary>
        public long ID
        {
            get;
            private set;
        }
        /// <summary>
        /// Add pre req task
        /// </summary>
        /// <param name="id">The id of the task to prereq</param>
        public void AddPreReq(long id)
        {
            _PreReqTasks.Add(id);
        }
        /// <summary>
        /// The time that the task was created.
        /// </summary>
        public DateTime UtcCreationTime
        {
            get;
            private set;
        }
        /// <summary>
        /// The time that the task started execution.
        /// This will be min time if it has not started.
        /// </summary>
        public DateTime UtcStartTime
        {
            get;
            private set;
        }
        /// <summary>
        /// The time that the task ended execution.
        /// This will be min time if it has not completed.
        /// </summary>
        public DateTime UtcEndTime
        {
            get;
            private set;
        }
        /// <summary>
        /// A list of tasks that must be executed before this one.
        /// </summary>
        public ReadOnlyCollection<long> PreReqTasks
        {
            get
            {
                return new ReadOnlyCollection<long>(_PreReqTasks);
            }
        }
        /// <summary>
        /// Can contain any additional data you would like to have with the task.
        /// This is good for moving data to the location of the callbacks.
        /// </summary>
        public object Tag
        {
            get;
            set;
        }
        /// <summary>
        /// The base class constructor for the Task class.
        /// </summary>
        public Task()
        {
            _PreReqTasks = new List<long>();
            ID = _GetNextID();
            UtcCreationTime = DateTime.UtcNow;
            UtcEndTime = DateTime.MinValue;
            UtcStartTime = DateTime.MinValue;
        }
        /// <summary>
        /// This executes the task.
        /// </summary>
        /// <param name="results">The out paramter that will contain the results.</param>
        /// <param name="error">The out paramter that will contain any error.</param>
        public void Execute(out TaskResults results, out TaskError error)
        {
            UtcStartTime = DateTime.UtcNow;
            __Execute(out results, out error);
            UtcEndTime = DateTime.UtcNow;
        }
        /// <summary>
        /// This is to be implemented by each task. It should not throw any exceptions.
        /// </summary>
        /// <param name="results">The out paramter that will contain the results.</param>
        /// <param name="error">The out paramter that will contain any error.</param>
        protected abstract void __Execute(out TaskResults results, out TaskError error);
        /// <summary>
        /// TaskExecutor calls this to mark the task as executed.
        /// </summary>
        /// <param name="eventargs">The event args to pass in the event.</param>
        public void TriggerTaskCompleted(TaskCompletedEventArgs eventargs)
        {
            if (TaskCompleted != null)
            {
                TaskCompleted(this, eventargs);
            }
        }
        /// <summary>
        /// TaskExecutor calls this to mark the task as errored.
        /// </summary>
        /// <param name="eventargs">The event args to pass in the event.</param>
        public void TriggerTaskErrored(TaskErroredEventArgs eventargs)
        {
            if (TaskErrored != null)
            {
                TaskErrored(this, eventargs);
            }
        }
    }
}
