using System.IO;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileConverting;

namespace Veeam.IntroductoryAssignment.Tasks
{
    class ReadFileChunkTask : ObservableTask
    {
        private readonly FileChunk _fileChunk;

        public ReadFileChunkTask(FileChunk fileChunk, ITaskCompletionObserver fileConverter)
            : base(fileConverter)
        {
            _fileChunk = fileChunk;
        }

        public override void Execute()
        {
            var fileName = _fileChunk.FileName;
            var chunkInfo = _fileChunk.Info;
            var data = _fileChunk.GetData();

            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(chunkInfo.Position, SeekOrigin.Begin);
                fileStream.Read(data, 0, chunkInfo.Length);
            }

            Observer.NotifyAboutTaskCompletion(_fileChunk);
        }
    }
}
