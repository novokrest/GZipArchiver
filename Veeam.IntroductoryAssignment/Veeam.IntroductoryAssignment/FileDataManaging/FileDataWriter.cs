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
}
