using System.IO;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileChunkManaging;

namespace Veeam.IntroductoryAssignment.FileDataManaging
{
    class FileDataWriter : FileNameHolder
    {
        public FileDataWriter(string fileName)
            : base(fileName)
        {
        }

        public virtual void WriteData(FileChunkInfo chunkInfo, byte[] data)
        {
            using (var fileStream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
            {
                fileStream.Seek(chunkInfo.Position, SeekOrigin.Begin);
                fileStream.Write(data, 0, chunkInfo.Length);
            }
        }
    }

    class ConcurrentFileDataWriter : FileDataWriter
    {
        private readonly object _lock = new object();

        public ConcurrentFileDataWriter(string fileName)
            : base(fileName)
        {
        }

        public override void WriteData(FileChunkInfo chunkInfo, byte[] data)
        {
            lock (_lock)
            {
                base.WriteData(chunkInfo, data);
            }
        }
    }
}
