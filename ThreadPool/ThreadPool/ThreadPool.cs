using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Veeam.TestTask
{
    class TaskExecutor
    {
        private readonly ITaskPool _taskPool;
        private Thread _handler;

        public TaskExecutor(ITaskPool taskPool)
        {
            _taskPool = taskPool;
        }

        public bool IsAborted { get; private set; }

        public void Run()
        {
            _handler = new Thread(() =>
            {
                while (!IsAborted)
                {
                    var task = _taskPool.GetTask();
                    if (task != null) ExecuteTask(task);
                }
            });
            _handler.Start();
        }

        public void Abort()
        {
            IsAborted = true;
        }

        private void ExecuteTask(ITask task)
        {
            try
            {
                Console.WriteLine("Start task: {0}", task);
                task.Execute();
            }
            catch (Exception e)
            {
                throw new TaskExecuteException(String.Format("Error occurred during task executing. Thread: {0}, Message: {1}", _handler.Name, e.Message));
            }
        }

        public void Wait()
        {
            _handler.Join();
        }
    }

    internal class TaskExecuteException : Exception
    {
        public TaskExecuteException(string message)
            : base(message)
        {
            
        }
    }

    interface ITaskPool
    {
        void AddTask(ITask task);
        ITask GetTask();
    }

    class PriorityTaskPool : ITaskPool
    {
        private readonly static ITaskPool instance = new PriorityTaskPool();

        private readonly Queue<ITask>[] _tasks; 
        private readonly object _lock = new object();
        private readonly IList<TaskExecutor> _executors = new List<TaskExecutor>(); 

        private PriorityTaskPool(int priorities = 1)
        {
            if (priorities < 0) throw new ArgumentException("Number of priorities could not be negative");
            _tasks = new Queue<ITask>[priorities];
            InitTaskQueues();
            InitTaskExecutors(Environment.ProcessorCount);
        }

        public static ITaskPool Instance
        {
            get { return instance; }
        }

        //TODO: add autoresetevent for waiting
        //TODO: implement taskexecutormanager
        public void Stop()
        {
            foreach (var executor in _executors)
            {
                executor.Abort();
            }
        }

        private void InitTaskExecutors(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                var executor = new TaskExecutor(this);
                _executors.Add(executor);
                executor.Run();
            }
        }

        private void InitTaskQueues()
        {
            for (var i = 0; i < _tasks.Length; ++i)
            {
                _tasks[i] = new Queue<ITask>();
            }
        }

        public ITask GetTask()
        {
            lock (_lock)
            {
                var task =_tasks.Where(taskQueue => taskQueue.Count > 0)
                    .Select(taskQueue => taskQueue.Dequeue())
                    .FirstOrDefault();
                if (task == null) Monitor.Wait(_lock);
                return task;
            }
        }

        public void AddTask(ITask task)
        {
            AddTask(task, 0);
        }

        public void AddTask(ITask task, int priority)
        {
            if (priority > _tasks.Length)
            {
                throw new ArgumentException($"Priority {priority} is not supported");
            }
            lock (_lock)
            {
                _tasks[priority].Enqueue(task);
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
