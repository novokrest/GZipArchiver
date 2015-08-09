using System;
using System.IO;

namespace Veeam.IntroductoryAssignment.FileChunkManaging.Splitting
{
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

        public FileSplitInfo GetFileSplitInfo()
        {
            var chunkLengths = GetChunkLengths();
            var splitInfo = new FileSplitInfo(chunkLengths.Length);
            for (long id = 0, position = 0; id < splitInfo.ChunkCount; id++)
            {
                var fileChunkInfo = new FileChunkInfo(id, chunkLengths[id])
                {
                    Position = position
                };
                splitInfo.Chunks[id] = fileChunkInfo;
                position += fileChunkInfo.Length;
            }
            return splitInfo;
        }

        protected abstract int[] GetChunkLengths();
    }

    internal class FixedSplitInfoExtractor : BaseFileSplitInfoExtractor
    {
        private static readonly int DefaultMaxChunkLength = 15*1024*1024;

        private readonly int _maxChunkLength;

        public FixedSplitInfoExtractor(string fileName)
            : this(fileName, DefaultMaxChunkLength)
        {
        }

        public FixedSplitInfoExtractor(string fileName, int maxChunkLength)
            : base(fileName)
        {
            _maxChunkLength = maxChunkLength;
        }

        protected override int[] GetChunkLengths()
        {
            var fileSize = GetFileSize();
            var chunkCount = fileSize % _maxChunkLength == 0 ? fileSize / _maxChunkLength : fileSize / _maxChunkLength + 1;
            
            var chunkLeghths = new int[chunkCount];
            for (long id = 0, total = 0; id < chunkCount; id++, total += _maxChunkLength)
            {
                chunkLeghths[id] = (int) Math.Min(_maxChunkLength, fileSize - total);
            }

            return chunkLeghths;
        }
    }

    internal class ArchiveSplitInfoExtractor : BaseFileSplitInfoExtractor
    {
        public ArchiveSplitInfoExtractor(string fileName)
            : base(fileName)
        {
        }

        protected override int[] GetChunkLengths()
        {
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(0, SeekOrigin.End);
                var headerReader = new ArchiveHeaderReverseReader(fileStream);
                var header = headerReader.ReadHeader();

                return header.ChunkLengths;
            }
        }
    }
}
