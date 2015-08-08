using Veeam.IntroductoryAssignment.Common;

namespace Veeam.IntroductoryAssignment.Tasks
{
    internal abstract class ObservableTask : ITask
    {
        private readonly ITaskCompletionObserver _observer;

        protected ObservableTask(ITaskCompletionObserver observer)
        {
            _observer = observer;
        }

        protected ITaskCompletionObserver Observer
        {
            get { return _observer; }
        }

        public abstract void Execute();
    }
}
