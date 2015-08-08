using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.Tasks;

namespace Veeam.IntroductoryAssignment.ThreadPool
{
    internal class TaskExecutor : ConsoleLoggable
    {
        private enum State
        {
            Runned,
            Waited,
            Stopped,
            Aborted
        }

        private readonly ITaskProducer _tasks;
        private readonly object _lock;

        private State _state;
        private Thread _handler;

        public TaskExecutor(ITaskProducer tasks, object synchLock)
        {
            _tasks = tasks;
            _lock = synchLock;
        }

        public void Run()
        {
            _handler = new Thread(() =>
            {
                while (_state == State.Runned)
                {
                    lock (_lock)
                    {
                        var task = _tasks.GetTask();
                        if (task != null) ExecuteTask(task);
                        else
                        {
                            Monitor.Wait(_lock);
                            if (_state == State.Aborted) Monitor.PulseAll(_lock);
                        }
                    }
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
                Log(String.Format("runned task {0}", task));
                task.Execute();
            }
            catch (Exception e)
            {
                throw new TaskExecuteException(
                    String.Format("Error occurred during task executing. TaskExecutor: {0}, Message: {1}",
                        this, e.Message), e);
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
