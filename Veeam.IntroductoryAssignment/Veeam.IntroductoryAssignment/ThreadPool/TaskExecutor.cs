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
        private State _state;
        private Thread _handler;

        public TaskExecutor(ITaskPool taskPool)
        {
            _taskPool = taskPool;
        }

        public void Run()
        {
            _handler = new Thread(() =>
            {
                while (_state == State.Runned)
                {
                    var task = _taskPool.GetNextTask(this);
                    if (task == null) continue;
                    try
                    {
                        task.Execute();
                    }
                    catch (Exception e)
                    {
                        _taskPool.Stop();
                        _taskPool.Exception = e;
                        Console.WriteLine("Error occurred during task executing. TaskExecutor: {0}, Message: {1}",
                                this, e.Message);
                    }
                }
                
            });

            _state = State.Runned;
            _handler.Start();
        }

        private void ExecuteTask(ITask task)
        {
            try
            {
                Log(String.Format("run task {0}", task));
                
                Log(String.Format("complete task {0}", task));
            }
            catch (Exception e)
            {

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
        }

        public void Wait()
        {
            _handler.Join();
        }

        public override string ToString()
        {
            return String.Format("TaskExecutor on thread {0}", _handler.ManagedThreadId);
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
