using System.Collections.Generic;
using System.Threading;

namespace Veeam.IntroductoryAssignment.Common
{
    interface ICompletionWaitable
    {
        void WaitForComplete();
    }

    class BaseCompletionWaitable : ICompletionWaitable
    {
        private readonly Dictionary<Thread, AutoResetEvent> _waiters = new Dictionary<Thread, AutoResetEvent>();

        public void WaitForComplete()
        {
            var thread = Thread.CurrentThread;
            var waitHandler = new AutoResetEvent(false);
            _waiters.Add(thread, waitHandler);
            waitHandler.WaitOne();
        }

        public void NotifyWaiters()
        {
            foreach (var waitHandler in _waiters.Values)
            {
                waitHandler.Set();
            }
        }
    }
}
