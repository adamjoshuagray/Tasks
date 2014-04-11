using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tasks
{
    public class TaskResults
    {
        public Task CompletedTask
        {
            get;
            private set;
        }
        public TaskResults(Task task)
        {
            CompletedTask = task;
        }
    }
}
