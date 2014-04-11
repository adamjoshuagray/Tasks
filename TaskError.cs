using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tasks
{
    public class TaskError
    {
        public Task ErroredTask
        {
            get;
            private set;
        }
        public TaskError(Task task)
        {
            ErroredTask = task;
        }
    }
}
