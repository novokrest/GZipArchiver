using System;
using System.Diagnostics;
using System.Threading;
using Veeam.IntroductoryAssignment.Common;

namespace Veeam.IntroductoryAssignment.ThreadPool
{
    internal class TaskExecutor : ConsoleLoggable
    {
        private enum State
        {
            Runned,
            Stopped,
            Aborted
        }

        private readonly ITaskPool _taskPool;
        private readonly object _lock;

        private State _state;
        private Thread _handler;

        public TaskExecutor(ITaskPool taskPool, object synchLock)
        {
            _taskPool = taskPool;
            _lock = synchLock;
        }

        public void Run()
        {
            _handler = new Thread(() =>
            {
                while (_state == State.Runned)
                {
                    ITask task;
                    lock (_lock)
                    {
                        task = _taskPool.GetTask();
                        if (task == null) 
                        {
                            Monitor.Wait(_lock);
                            if (_state == State.Aborted) Monitor.PulseAll(_lock);
                        }
                    }
                    if (task != null) ExecuteTask(task);
                }
                Log("Exit");
            });

            _state = State.Runned;
            _handler.Start();
        }

        private void ExecuteTask(ITask task)
        {
            try
            {
                Log(String.Format("run task {0}", task));
                task.Execute();
                Log(String.Format("complete task {0}", task));
            }
            catch (Exception e)
            {
                Abort();
                Console.WriteLine("Error occurred during task executing. TaskExecutor: {0}, Message: {1}",
                        this, e.Message);
            }
        }

        public void Abort()
        {
            _state = State.Aborted;
            Log("Aborted");
        }

        public void Stop()
        {
            _state = State.Stopped;
            Log("Stopped");
        }

        public void Wait()
        {
            Log("Start waiting");
            _handler.Join();
            Log("End waiting");
        }

        public override string ToString()
        {
            return base.ToString() + String.Format("on thread {0}", _handler.ManagedThreadId);
        }
    }

    internal class TaskExecuteException : Exception
    {
        public TaskExecuteException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
