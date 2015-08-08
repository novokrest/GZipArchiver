using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Veeam.IntroductoryAssignment.FileContentManagers;

namespace Veeam.IntroductoryAssignment.Tasks
{
    class FileAssembleTask : ITask
    {
        private readonly FileChunk _fileChunk;
        private readonly FileAssembler _fileChunkOwner;

        public FileAssembleTask(FileChunk fileChunk, FileAssembler fileChunkOwner)
        {
            _fileChunk = fileChunk;
            _fileChunkOwner = fileChunkOwner;
        }

        public void Execute()
        {
            var fileName = _fileChunk.FileName;
            var chunkInfo = _fileChunk.Info;
            var data = _fileChunk.GetData();

            using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fileStream.Seek(chunkInfo.Position, SeekOrigin.Begin);
                fileStream.Write(data, 0, chunkInfo.Length);
            }

            _fileChunkOwner.NotifyAboutTaskCompletion(_fileChunk);
        }
    }
}
