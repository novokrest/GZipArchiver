using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Veeam.IntroductoryAssignment.FileDataManaging;

namespace Veeam.IntroductoryAssignment.Common
{
    class FileDataReader : FileNameHolder
    {
         public FileDataReader(string fileName)
             : base(fileName)
        {
        }

        public virtual void ReadData(FileChunkInfo chunkInfo, byte[] data)
        {
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(chunkInfo.Position, SeekOrigin.Begin);
                fileStream.Read(data, 0, chunkInfo.Length);
            }
        }
    }

    class ConcurrentFileDataReader : FileDataReader
    {
        private readonly object _lock = new object();

        public ConcurrentFileDataReader(string fileName) 
            : base(fileName)
        {
        }

        public override void ReadData(FileChunkInfo chunkInfo, byte[] data)
        {
            lock (_lock)
            {
                base.ReadData(chunkInfo, data);
            }
        }
    }
}
