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

    interface ITaskPool : IScheduleStrategy
    {
        void Start();
        void Stop();
        void Abort();
        void WakeUpAll();
        void WaitForCompleting();
        Exception Exception { get; set; }
        TaskPoolState State { get; set; }
    }

    interface ITaskProducer
    {
        ITask GetNextTask(TaskExecutor executor);
    }

    interface ITaskConsumer
    {
        void AddTask(ITask task, int priority);
    }

    interface IScheduleStrategy : ITaskConsumer, ITaskProducer
    {
    }

    abstract class BaseScheduleStrategy : IScheduleStrategy
    {
        private readonly object _lock;

        protected BaseScheduleStrategy(object taskPoolLock)
        {
            _lock = taskPoolLock;
        }

        public object Lock
        {
            get { return _lock; }
        }

        public abstract void AddTask(ITask task, int priority);
        public abstract ITask GetNextTask(TaskExecutor executor);
    }

    class NormalScheduleStrategy : BaseScheduleStrategy
    {
        private readonly PriorityQueue<ITask> _tasks;

        public NormalScheduleStrategy(object taskPoolLock, PriorityQueue<ITask> tasks)
            : base(taskPoolLock)
        {
            _tasks = tasks;
        }

        public override void AddTask(ITask task, int priority)
        {
            lock (Lock)
            {
                _tasks.Enqueue(task, priority);
                Monitor.PulseAll(Lock);
            }
        }

        public override ITask GetNextTask(TaskExecutor executor)
        {
            lock (Lock)
            {
                var task = _tasks.Dequeue();
                if (task == null) Monitor.Wait(Lock);
                return task;
            }
        }
    }

    class TerminatingScheduleStrategy : BaseScheduleStrategy
    {
        public TerminatingScheduleStrategy(object taskPoolLock)
            : base(taskPoolLock)
        {
        }

        public override void AddTask(ITask task, int priority)
        {
            lock (Lock)
            {
                Monitor.PulseAll(Lock);
            }
        }

        public override ITask GetNextTask(TaskExecutor executor)
        {
            return new TerminateTask(executor, Lock);
        }
    }

    class PriorityTaskPool : ConsoleLoggable, ITaskPool
    {
        private readonly PriorityQueue<ITask> _tasks = new PriorityQueue<ITask>();

        private readonly static ITaskPool instance = new PriorityTaskPool();

        private IScheduleStrategy _scheduleStrategy;

        private readonly object _lock = new object();

        private readonly IList<TaskExecutor> _executors = new List<TaskExecutor>();

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
            _scheduleStrategy = new NormalScheduleStrategy(_lock, _tasks);

            Console.CancelKeyPress += delegate
            {
                if (State == TaskPoolState.Started)
                {
                    Stop();
                }
            };

            for (var i = 0; i < executorsCount; i++)
            {
                var executor = new TaskExecutor(this);
                _executors.Add(executor);
            }
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
            //_scheduleStrategy = new TerminatingScheduleStrategy(_lock);

            Abort();
            WakeUpAll();
            //WaitForCompleting();
            State = TaskPoolState.Stopped;
            Console.WriteLine("!!!STOOOOOP!!!!");
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
            return _scheduleStrategy.GetNextTask(executor);
        }

        public void AddTask(ITask task, int priority)
        {
            _scheduleStrategy.AddTask(task, priority);
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
                Log(String.Format("executor {0} EXIT", executor));
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
