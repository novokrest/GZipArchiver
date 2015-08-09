using System.Collections.Generic;
using System.Linq;

namespace Veeam.IntroductoryAssignment.Util
{
    class PriorityQueue<T>
    {
        private readonly SortedDictionary<int, Queue<T>> _queues = new SortedDictionary<int, Queue<T>>();
        private long _count;

        public long Count
        {
            get
            {
                return _count;
            }
        }

        public void Enqueue(T item, int priority)
        {
            GetQueue(priority).Enqueue(item);
            _count++;
        }

        private Queue<T> GetQueue(int priority)
        {
            if (!_queues.ContainsKey(priority))
            {
                _queues.Add(priority, new Queue<T>());
            }
            return _queues[priority];
        }

        public T Dequeue()
        {
            var highPriorityQueue = _queues.Values.LastOrDefault(queue => queue.Count > 0);
            if (highPriorityQueue == null) return default(T);
            var item = highPriorityQueue.Dequeue();
            _count--;
            return item;
        }
    }
}
