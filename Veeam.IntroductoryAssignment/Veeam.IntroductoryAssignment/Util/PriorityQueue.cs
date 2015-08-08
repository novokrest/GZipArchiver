using System.Collections.Generic;
using System.Linq;

namespace Veeam.IntroductoryAssignment.Util
{
    class PriorityQueue<T>
    {
        private readonly SortedDictionary<int, Queue<T>> _queues = new SortedDictionary<int, Queue<T>>();

        public void Enqueue(T item, int priority)
        {
            GetQueue(priority).Enqueue(item);
        }

        private Queue<T> GetQueue(int priority)
        {
            if (!_queues.ContainsKey(priority))
            {
                _queues.Add(priority, new Queue<T>());
            }
            return _queues[priority];
        }

        //TODO: remove Queue if it is Empty
        public T Dequeue()
        {
            var highPriorityQueue = _queues.Values.LastOrDefault(queue => queue.Count > 0);
            return highPriorityQueue != null ? highPriorityQueue.Dequeue() : default(T);
        }
    }
}
