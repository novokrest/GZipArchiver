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

    internal abstract class FileSplitter : FileDataHolder
    {
        private readonly FileChunkInfo[] _chunkInfos;

        protected class SplitInfo
        {
            public SplitInfo(long chunkCount, int[] chunkLengths)
            {
                ChunkCount = chunkCount;
                ChunkLengths = chunkLengths;
            }

            public long ChunkCount { get; }
            public int[] ChunkLengths { get; }
        }

        protected abstract SplitInfo Split();

        protected FileSplitter(string fileName)
            : base(fileName)
        {
            var splitInfo = Split();

            _chunkInfos = Split();
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

        //private FileChunkInfo[] Split()
        //{
        //    var chunkSize = GetMagicChunkLength();
        //    var chunkCount = GetFileSize()/chunkSize + 1;
        //    var chunkInfos = new FileChunkInfo[chunkCount];

        //    for (long id = 0, position = 0; id < chunkCount; id++)
        //    {
        //        chunkInfos[id] = new FileChunkInfo(id, (int) Math.Min(maxChunkLength, fileSize - position))
        //        {
        //            Position = position
        //        };
        //        position += chunkInfos[id];
        //    }
        //    return chunkInfos;
        //}

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
            _chunkInfos = Split();
        }

        private FileChunkInfo[] Split()
        {
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                var header = Header.Read(fileStream);
                var chunkInfos = new FileChunkInfo[header.ChunkCount];
                for (long id = 0, position = 0; id < header.ChunkCount; ++id)
                {
                    chunkInfos[id] = new FileChunkInfo(id, header.ChunkLengths[id])
                    {
                        Position = position
                    };
                    position += chunkInfos[id].Length;
                }
                return chunkInfos;
            }
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
