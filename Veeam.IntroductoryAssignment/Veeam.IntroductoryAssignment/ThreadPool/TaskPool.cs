using System;
using System.Collections.Generic;
using System.Threading;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.Util;

namespace Veeam.IntroductoryAssignment.ThreadPool
{
    enum TaskPoolState
    {
        Started,
        Stopped
    }

    interface ITaskProducer
    {
        ITask GetNextTask(TaskExecutor executor);
    }

    interface ITaskConsumer
    {
        void AddTask(ITask task, int priority);
    }

    interface ITaskPool : ITaskProducer, ITaskConsumer
    {
        void Start();
        void Stop();
        void Abort();
        void WakeUpAll();
        void WaitForCompleting();
        Exception Exception { get; set; }
        TaskPoolState State { get; set; }
    }

    class PriorityTaskPool : ConsoleLoggable, ITaskPool
    {
        private readonly static ITaskPool instance = new PriorityTaskPool();

        private readonly IList<TaskExecutor> _executors = new List<TaskExecutor>();
        private readonly PriorityQueue<ITask> _tasks = new PriorityQueue<ITask>();
        private readonly object _lock = new object();

        public static ITaskPool Instance
        {
            get { return instance; }
        }

        private PriorityTaskPool()
            : this(Environment.ProcessorCount)
        {
        }

        private PriorityTaskPool(int executorsCount)
        {
            for (var i = 0; i < executorsCount; i++)
            {
                var executor = new TaskExecutor(this);
                _executors.Add(executor);
            }

            Console.CancelKeyPress += delegate
            {
                if (State == TaskPoolState.Started)
                {
                    Stop();
                }
            };
        }

        public void Start()
        {
            State = TaskPoolState.Started;
            foreach (var executor in _executors)
            {
                executor.Run();
            }
        }

        public void Stop()
        {
            Abort();
            WakeUpAll();
            State = TaskPoolState.Stopped;
        }

        public void WakeUpAll()
        {
            lock (_lock)
            {
                Monitor.PulseAll(_lock);
            }
        }

        public ITask GetNextTask(TaskExecutor executor)
        {
            lock (_lock)
            {
                var task = _tasks.Dequeue();
                if (task == null) Monitor.Wait(_lock);
                return task;
            }
        }

        public void AddTask(ITask task, int priority)
        {
            lock (_lock)
            {
                _tasks.Enqueue(task, priority);
                Monitor.PulseAll(_lock);
            }
        }

        public void Abort()
        {
            foreach (var executor in _executors)
            {
                executor.Abort();
            }
        }

        public void WaitForCompleting()
        {
            foreach (var executor in _executors)
            {
                executor.Wait();
            }
        }

        public Exception Exception { get; set; }
        public TaskPoolState State { get; set; }

        public override string ToString()
        {
            return String.Format("TaskPool[ Tasks: {0}, Executors: {1} ]", _tasks.Count, _executors.Count);
        }
    }
}
