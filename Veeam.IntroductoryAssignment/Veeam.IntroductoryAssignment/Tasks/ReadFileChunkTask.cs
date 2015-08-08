using System.IO;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileConverting;

namespace Veeam.IntroductoryAssignment.Tasks
{
    class ReadFileChunkTask : ITask
    {
        private readonly FileChunk _fileChunk;
        private readonly FileConverter _fileConverter;

        public ReadFileChunkTask(FileChunk fileChunk, FileConverter fileConverter)
        {
            _fileChunk = fileChunk;
            _fileConverter = fileConverter;
        }

        public void Execute()
        {
            var fileName = _fileChunk.FileName;
            var chunkInfo = _fileChunk.Info;
            var data = _fileChunk.GetData();

            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                fileStream.Read(data, 0, chunkInfo.Length);
            }

            _fileConverter.NotifyAboutTaskCompletion(_fileChunk);
        }
    }
}
