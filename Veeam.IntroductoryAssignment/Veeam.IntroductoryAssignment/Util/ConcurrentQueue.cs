using System;
using System.Collections.Generic;

namespace Veeam.IntroductoryAssignment.Util
{
    interface IConcurrentQueue<T>
    {
        void Enqueue(T item);
        T Dequeue();
    }

    class DummyConcurrentQueue<T> : IConcurrentQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();

        private readonly object _lockEnq = new object();
        private readonly object _lockDeq = new object();

        public void Enqueue(T item)
        {
            lock (_lockEnq)
            {
                _queue.Enqueue(item);   
            }
        }

        public T Dequeue()
        {
            lock (_lockDeq)
            {
                return _queue.Dequeue();   
            }
        }
    }

    class ConcurrentQueue<T> : IConcurrentQueue<T>
    {
        public class Node
        {
            public T Item { get; private set; }
            public Node Next { get; set; }

            public Node(T item, Node next)
            {
                Item = item;
                Next = next;
            }
        }

        private readonly object _lockEnq = new object();
        private readonly object _lockDeq = new object();

        private readonly Node _head;
        private Node _tail;

        public ConcurrentQueue()
        {
            _head = _tail = new Node(default(T), null);
            _head.Next = _tail;
        }

        public void Enqueue(T item)
        {
            var node = new Node(item, null);
            lock (_lockEnq)
            {
                _tail.Next = node;
                _tail = node;   
            }
        }

        public T Dequeue()
        {
            if (_head.Next == null)
            {
                throw new InvalidOperationException("Queue is empty");
            }

            lock (_lockDeq)
            {
                var node = _head.Next;
                if (_tail == node)
                {
                    _tail = _head;
                }
                _head.Next = node.Next;
                return node.Item;
            }
        }
    }
}
