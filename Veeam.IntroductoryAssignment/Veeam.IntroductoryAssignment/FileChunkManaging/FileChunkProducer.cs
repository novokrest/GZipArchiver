using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileDataManaging;
using Veeam.IntroductoryAssignment.FileSplitting;

namespace Veeam.IntroductoryAssignment.FileChunkManaging
{
    internal abstract class FileChunkProducer : FileNameHolder
    {
        private readonly FileSplitInfo _fileSplitInfo;
        private readonly FileDataHolder _fileDataHolder;
        private readonly FileDataReader _fileDataReader;
        private readonly FileDataWriter _fileDataWriter;

        protected FileChunkProducer(string fileName, FileSplitInfo fileSplitInfo, FileDataHolder fileDataHolder, FileDataReader fileDataReader, FileDataWriter fileDataWriter)
            : base(fileName)
        {
            _fileSplitInfo = fileSplitInfo;
            _fileDataHolder = fileDataHolder;
            _fileDataReader = fileDataReader;
            _fileDataWriter = fileDataWriter;
        }

        public FileSplitInfo SplitInfo
        {
            get { return _fileSplitInfo; }
        }

        public FileDataHolder DataHolder
        {
            get { return _fileDataHolder; }
        }

        public FileDataReader DataReader
        {
            get { return _fileDataReader; }
        }

        public FileDataWriter DataWriter
        {
            get { return _fileDataWriter; }
        }

        public IEnumerable<FileChunk> GetFileChunks()
        {
            for (long id = 0; id < SplitInfo.ChunkCount; id++)
            {
                var chunkInfo = SplitInfo.Chunks[id];
                yield return CreateFileChunk(chunkInfo);
            }
        }

        protected FileChunk CreateFileChunk(FileChunkInfo chunkInfo)
        {
            return new FileChunk(FileName, chunkInfo, DataHolder, DataReader, DataWriter);
        }

        public long GetChunkCount()
        {
            return SplitInfo.ChunkCount;
        }
    }
}
