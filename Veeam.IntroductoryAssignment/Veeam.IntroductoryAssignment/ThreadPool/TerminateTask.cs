using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Veeam.IntroductoryAssignment.Common;

namespace Veeam.IntroductoryAssignment.ThreadPool
{
    class ExceptionTask : ITask
    {
        public void Execute()
        {
            throw new Exception("ExceptionTask");
        }
    }

    class TerminateTask : ITask
    {
        private readonly TaskExecutor _executor;
        private readonly object _wakeUpLock;

        public TerminateTask(TaskExecutor executor, object wakeUpLock)
        {
            _executor = executor;
            _wakeUpLock = wakeUpLock;
        }

        public void Execute()
        {
            _executor.Abort();
            _executor.Log("Fake Task");
            lock (_wakeUpLock)
            {
                _executor.Log("INSIDE WAKE UP LOG");
                Monitor.PulseAll(_wakeUpLock);
            }
        }
    }
}
