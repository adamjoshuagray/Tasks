using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tasks
{
    public class PreReqTaskError : TaskError
    {
        public long PreReqTaskID
        {
            get;
            private set;
        }
        public PreReqTaskError(Task task, long prereqtaskid)
            : base(task)
        {
            PreReqTaskID = prereqtaskid;
        }
    }
}
