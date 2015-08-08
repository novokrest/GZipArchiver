using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Veeam.IntroductoryAssignment.FileContentManagers
{
    internal class FileSplitInfo
    {
        private readonly long _chunkCount;
        private readonly FileChunkInfo[] _chunks;

        public FileSplitInfo(long chunkCount)
        {
            _chunkCount = chunkCount;
            _chunks = new FileChunkInfo[chunkCount];
        }

        public long ChunkCount
        {
            get { return _chunkCount; }
        }

        public FileChunkInfo[] Chunks
        {
            get { return _chunks; }
        }
    }

    internal interface IFileSplitInfoExtractor
    {
        FileSplitInfo GetFileSplitInfo();
    }

    internal abstract class BaseFileSplitInfoExtractor : IFileSplitInfoExtractor
    {
        private readonly string _fileName;

        protected BaseFileSplitInfoExtractor(string fileName)
        {
            _fileName = fileName;
        }

        public string FileName
        {
            get { return _fileName; }
        }

        protected long GetFileSize()
        {
            return new FileInfo(FileName).Length;
        }

        public abstract FileSplitInfo GetFileSplitInfo();
    }
}
