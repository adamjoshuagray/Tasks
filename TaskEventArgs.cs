using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tasks
{
    public class TaskCompletedEventArgs : EventArgs
    {
        public TaskResults Results
        {
            get;
            private set;
        }
        public TaskCompletedEventArgs(TaskResults results)
        {
            Results = results;
        }
    }
    public class TaskErroredEventArgs : EventArgs
    {
        public TaskError Error
        {
            get;
            private set;
        }
        public TaskErroredEventArgs(TaskError error)
        {
            Error = error;
        }
    }
}
