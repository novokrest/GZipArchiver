using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace Veeam.IntroductoryAssignment.FileContentManagers
{
    interface IFileSplitter
    {
        IEnumerable<FileChunk> GetFileChunks();
    }

    internal abstract class FileSplitter : FileDataHolder
    {
        private readonly FileSplitInfo _fileSplitInfo;

        protected FileSplitter(string fileName, IFileSplitInfoExtractor splitInfoExtractor)
            : base(fileName)
        {
            _fileSplitInfo = splitInfoExtractor.GetFileSplitInfo();
        }

        public override byte[] GetData(FileChunkInfo chunkInfo)
        {
            if (!HasData(chunkInfo))
            {
                ReadData(chunkInfo);
            }
            return base.GetData(chunkInfo);
        }

        private void ReadData(FileChunkInfo chunkInfo)
        {
            var data = new byte[chunkInfo.Length];
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileStream.Seek(chunkInfo.Position, SeekOrigin.Begin);
                fileStream.ReadAll(data, 0, data.Length);
            }
            AddData(chunkInfo, data);
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
            return new FileChunk(FileName, chunkInfo, this);
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
