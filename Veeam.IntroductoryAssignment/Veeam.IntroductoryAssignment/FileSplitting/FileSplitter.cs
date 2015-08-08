using System.Collections.Generic;
using Veeam.IntroductoryAssignment.Common;

namespace Veeam.IntroductoryAssignment.FileSplitting
{
    interface IFileSplitter
    {
        IEnumerable<FileChunk> GetFileChunks();
    }

    internal abstract class FileSplitter
    {
        private readonly string _fileName;
        private readonly FileSplitInfo _fileSplitInfo;
        private readonly FileDataHolder _fileDataHolder;

        protected FileSplitter(string fileName, IFileSplitInfoExtractor splitInfoExtractor)
        {
            _fileName = fileName;
            _fileSplitInfo = splitInfoExtractor.GetFileSplitInfo();
            _fileDataHolder = new FileDataHolder(fileName);
        }

        public IEnumerable<FileChunk> GetFileChunks()
        {
            for (long id = 0; id < _fileSplitInfo.ChunkCount; id++)
            {
                var chunkInfo = _fileSplitInfo.Chunks[id];
                yield return GetFileChunk(chunkInfo);
            }
        }

        private FileChunk GetFileChunk(FileChunkInfo chunkInfo)
        {
            return new FileChunk(_fileName, chunkInfo, _fileDataHolder);
        }

        public long GetChunkCount()
        {
            return _fileSplitInfo.ChunkCount;
        }
    }

    internal class RegularFileSplitter : FileSplitter
    {
        public RegularFileSplitter(string fileName)
            : base(fileName, new FixedSplitInfoExtractor(fileName))
        {
            
        }
    }

    internal class ArchiveFileSplitter : FileSplitter
    {
        public ArchiveFileSplitter(string fileName)
            : base(fileName, new ArchiveSplitInfoExtractor(fileName))
        {
            
        }
    }
}
