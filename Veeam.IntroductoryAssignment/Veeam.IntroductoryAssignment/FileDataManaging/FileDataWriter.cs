using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Veeam.IntroductoryAssignment.FileDataManaging;

namespace Veeam.IntroductoryAssignment.Common
{
    class FileDataWriter : FileNameHolder
    {
        public FileDataWriter(string fileName)
            : base(fileName)
        {
        }

        public virtual void WriteData(FileChunkInfo chunkInfo, byte[] data)
        {
            using (var fileStream = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
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
