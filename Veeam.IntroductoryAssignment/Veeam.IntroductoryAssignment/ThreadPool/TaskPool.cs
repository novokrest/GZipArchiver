using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Veeam.IntroductoryAssignment.Tasks;
using Veeam.IntroductoryAssignment.Util;

namespace Veeam.IntroductoryAssignment.ThreadPool
{
    interface ITaskPool : ITaskProducer, ITaskConsumer
    {
        void Start();
        void Stop();
    }

    interface ITaskProducer
    {
        ITask GetTask();
    }

    interface ITaskConsumer
    {
        void AddTask(ITask task, int priority);
        void AddTask(ITask task);
    }

    class PriorityTaskPool : ITaskPool
    {
        private readonly static ITaskPool instance = new PriorityTaskPool();

        private readonly PriorityQueue<ITask> _tasks = new PriorityQueue<ITask>();
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
            for (var i = 0; i < executorsCount; i++)
            {
                var executor = new TaskExecutor(this, _lock);
                _executors.Add(executor);
            }
        }

        public void Start()
        {
            foreach (var executor in _executors)
            {
                executor.Run();
            }
        }

        public void Stop()
        {
            foreach (var executor in _executors)
            {
                executor.Abort();
            }
            
            lock (_lock)
            {
                Monitor.PulseAll(_lock);
            }

            foreach (var executor in _executors)
            {
                executor.Wait();
            }
        }

        public ITask GetTask()
        {
            return _tasks.Dequeue();
        }

        public void AddTask(ITask task)
        {
            AddTask(task, 0);
        }

        public void AddTask(ITask task, int priority)
        {
            lock (_lock)
            {
                _tasks.Enqueue(task, priority);
                Monitor.Pulse(_lock);
            }
        }

        public void Wait()
        {
            foreach (var executor in _executors)
            {
                executor.Wait();
            }
        }
    }
}
