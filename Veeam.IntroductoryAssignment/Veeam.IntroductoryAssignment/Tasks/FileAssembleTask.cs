using System.IO;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileAssembling;

namespace Veeam.IntroductoryAssignment.Tasks
{
    class FileAssembleTask : ObservableTask
    {
        private readonly FileChunk _fileChunk;

        public FileAssembleTask(FileChunk fileChunk, FileAssembler fileAssembler)
            : base(fileAssembler)
        {
            _fileChunk = fileChunk;
        }

        public override void Execute()
        {
            var fileName = _fileChunk.FileName;
            var chunkInfo = _fileChunk.Info;
            var data = _fileChunk.GetData();

            using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fileStream.Seek(chunkInfo.Position, SeekOrigin.Begin);
                fileStream.Write(data, 0, chunkInfo.Length);
            }

            _fileChunk.ReleaseData();
            Observer.NotifyAboutTaskCompletion(_fileChunk);
        }
    }
}
