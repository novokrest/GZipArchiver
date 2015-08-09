using System.IO;
using Veeam.IntroductoryAssignment.Common;

namespace Veeam.IntroductoryAssignment.Tasks
{
    class FileAssembleTask : ObservableTask
    {
        private readonly FileChunk _fileChunk;

        public FileAssembleTask(FileChunk fileChunk, ITaskCompletionObserver observer)
            : base(observer)
        {
            _fileChunk = fileChunk;
        }

        public override void Execute()
        {
            _fileChunk.WriteData();
            _fileChunk.ReleaseData();
            Observer.NotifyAboutTaskCompletion(_fileChunk);
        }
    }
}
