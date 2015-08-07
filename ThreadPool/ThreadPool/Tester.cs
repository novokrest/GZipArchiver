using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Veeam.TestTask
{
    class A
    { }

    class Tester
    {
        public static void Test()
        {
            TestConcurrentQueue(new ConcurrentQueue<int>());
            TestConcurrentQueue(new DummyConcurrentQueue<int>());

            new object();
        }
        public static void TestConcurrentQueue(IConcurrentQueue<int> queue)
        {
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            Debug.Assert(queue.Dequeue() == 1);
            Debug.Assert(queue.Dequeue() == 2);
            queue.Enqueue(4);
            Debug.Assert(queue.Dequeue() == 3);
            Debug.Assert(queue.Dequeue() == 4);
            queue.Enqueue(5);
            Debug.Assert(queue.Dequeue() == 5);
            try
            {
                queue.Dequeue();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
