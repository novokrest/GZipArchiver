using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Veeam.TestTask;

namespace ThreadPool
{
    interface IFileSplitter
    {
        IEnumerable<FileChunk> GetFileChunks();
    }

    class FileSplitter : FileDataHolder
    {
        private FileChunkInfo[] _chunkInfos;

        public FileSplitter(string fileName)
            : base(fileName)
        {
            Split(GetMagicChunkLength());
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

        private void Split(long maxChunkLength)
        {
            var fileSize = GetFileSize();
            _chunkInfos = new FileChunkInfo[fileSize / maxChunkLength + 1];

            for (long position = 0, id = 0; position < fileSize; position += maxChunkLength, id++)
            {
                _chunkInfos[id] = new FileChunkInfo(id, (int)Math.Min(maxChunkLength, fileSize - position))
                {
                    Position = position
                };
            }
        }

        public IEnumerable<FileChunk> GetFileChunks()
        {
            var chunkSize = GetMagicChunkLength();
            return GetFileChunks(chunkSize);
        }

        private int GetMagicChunkLength()
        {
            return 100 * 1024 * 1024;
        }

        private long GetFileSize()
        {
            return new FileInfo(FileName).Length;
        }

        public IEnumerable<FileChunk> GetFileChunks(long maxChunkSize)
        {
            return _chunkInfos.Select(GetFileChunk);
        }

        private FileChunk GetFileChunk(FileChunkInfo chunkInfo)
        {
            return new FileChunk(FileName, chunkInfo, this);
        }
    }

    class SmartFileSplitter : FileSplitter
    {
        private readonly FileChunkInfo[] _chunkInfos;

        public SmartFileSplitter(string fileName) 
            : base(fileName)
        {
            _chunkInfos = new FileChunkInfo[GetChunkCount()];
        }

        public class Header
        {
            public Header(long chunkCount, int[] chunkLengths)
            {
                ChunkCount = chunkCount;
                ChunkLengths = chunkLengths;
            }

            public long ChunkCount { get; }
            public int[] ChunkLengths { get; }

            public static Header Read(Stream stream)
            {
                var reader = new BinaryReader(stream);
                var count = reader.ReadInt32();
                var lengths = new int[count];
                for (var i = 0; i < count; ++i)
                {
                    lengths[i] = reader.ReadInt32();
                }
                return new Header(count, lengths);
            }

            public static void Write(Header header, Stream stream)
            {
                var writer = new BinaryWriter(stream);
                writer.Write(header.ChunkCount);
                for (int i = 0; i < header.ChunkCount; i++)
                {
                    writer.Write(header.ChunkLengths[i]);
                }
            }
        }

        private Header ReadHeader()
        {
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var headerReader = new BinaryReader(fileStream);
                var chunkCount = headerReader.ReadInt32();
            }
        }

        private int GetChunkCount()
        {

        }
    }

}
