using System.IO;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileChunkManaging;

namespace Veeam.IntroductoryAssignment.FileDataManaging
{
    class FileDataReader : FileNameHolder
    {
         public FileDataReader(string fileName)
             : base(fileName)
        {
        }

        public virtual void ReadData(FileChunkInfo chunkInfo, byte[] data)
        {
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileStream.Seek(chunkInfo.Position, SeekOrigin.Begin);
                fileStream.Read(data, 0, chunkInfo.Length);
            }
        }
    }
}
