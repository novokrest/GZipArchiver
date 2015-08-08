using System.IO;
using Veeam.IntroductoryAssignment.Common;

namespace Veeam.IntroductoryAssignment.Tasks
{
    class FileAssembleTask : ObservableTask
    {
        private readonly static object _lock = new object();

        private readonly FileChunk _fileChunk;

        public FileAssembleTask(FileChunk fileChunk, ITaskCompletionObserver observer)
            : base(observer)
        {
            _fileChunk = fileChunk;
        }

        public override void Execute()
        {
            var fileName = _fileChunk.FileName;
            var chunkInfo = _fileChunk.Info;
            var data = _fileChunk.GetData();

            lock (_lock)
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    fileStream.Seek(chunkInfo.Position, SeekOrigin.Begin);
                    fileStream.Write(data, 0, chunkInfo.Length);
                }   
            }

            _fileChunk.ReleaseData();
            Observer.NotifyAboutTaskCompletion(_fileChunk);
        }
    }
}
