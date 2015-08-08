using System;
using System.Threading;
using Veeam.IntroductoryAssignment.Common;

namespace Veeam.IntroductoryAssignment.Tasks.Decorators
{
    internal class VerboseTask : ITask
    {
        private readonly ITask _task;

        public VerboseTask(ITask task)
        {
            _task = task;
        }

        public void Execute()
        {
            Console.WriteLine("{0} on thread {1} is started", _task, Thread.CurrentThread.ManagedThreadId);
            _task.Execute();
            Console.WriteLine("{0} on thread {1} has been executed", _task, Thread.CurrentThread.ManagedThreadId);
        }

        public override string ToString()
        {
            return _task.ToString();
        }
    }
}
