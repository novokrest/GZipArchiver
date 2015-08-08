using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Veeam.IntroductoryAssignment.FileContentManagers
{
    internal class ArchiveHeader
    {
        private readonly long _chunkCount;
        private readonly int[] _chunkLengths;

        public ArchiveHeader(long chunkCount)
        {
            _chunkCount = chunkCount;
            _chunkLengths = new int[chunkCount];
        }

        public long ChunkCount
        {
            get { return _chunkCount; }
        }

        public int[] ChunkLengths
        {
            get { return _chunkLengths; }
        }

        public int GetSize()
        {
            return sizeof(long) + sizeof(int) * ChunkLengths.Length;
        }

        public static ArchiveHeader Create(FileSplitInfo fileSplitInfo)
        {
            var header = new ArchiveHeader(fileSplitInfo.ChunkCount);
            for (int id = 0; id < fileSplitInfo.ChunkCount; id++)
            {
                header.ChunkLengths[id] = fileSplitInfo.Chunks[id].Length;
            }
            return header;
        }
    }

    interface IArchiveHeaderReader
    {
        ArchiveHeader ReadHeader();
    }

    //TODO: make Serializable
    internal class ArchiveHeaderReader : IArchiveHeaderReader
    {
        private readonly Stream _stream;

        public ArchiveHeaderReader(Stream stream)
        {
            _stream = stream;
        }

        public ArchiveHeader ReadHeader()
        {
            var binaryReader = new BinaryReader(_stream);
            var chunkCount = binaryReader.ReadInt64();
            var header = new ArchiveHeader(chunkCount);
            for (long i = 0; i < header.ChunkCount; i++)
            {
                header.ChunkLengths[i] = binaryReader.ReadInt32();
            }
            return header;
        }
    }

    internal class ArchiveHeaderReverseReader : IArchiveHeaderReader
    {
        private readonly Stream _stream;

        public ArchiveHeaderReverseReader(Stream stream)
        {
            _stream = stream;
        }

        public ArchiveHeader ReadHeader()
        {
            var binaryReader = new BinaryReader(_stream);

            _stream.Seek(-8, SeekOrigin.Current);
            var chunkCount = binaryReader.ReadInt64();
            _stream.Seek(-8, SeekOrigin.Current);
            
            var header = new ArchiveHeader(chunkCount);
            for (long i = 0; i < chunkCount; i++)
            {
                _stream.Seek(-4, SeekOrigin.Current);
                header.ChunkLengths[chunkCount - i - 1] = binaryReader.ReadInt32();
                _stream.Seek(-4, SeekOrigin.Current);
            }

            return header;
        }
    }

    internal interface IArchiveHeaderWriter
    {
        void WriteHeader(ArchiveHeader header);
    }

    internal class ArchiveHeaderWriter : IArchiveHeaderWriter
    {
        private readonly Stream _stream;

        public ArchiveHeaderWriter(Stream stream)
        {
            _stream = stream;
        }

        public void WriteHeader(ArchiveHeader header)
        {
            var binaryWriter = new BinaryWriter(_stream);
            for (long i = 0; i < header.ChunkCount; i++)
            {
                binaryWriter.Write(header.ChunkLengths[i]);
            }
            binaryWriter.Write(header.ChunkCount);
        }
    }

    internal class ArchiveSplitInfoExtractor : BaseFileSplitInfoExtractor
    {
        public ArchiveSplitInfoExtractor(string fileName)
            : base(fileName)
        {
        }

        public override FileSplitInfo GetFileSplitInfo()
        {
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            {
                fileStream.Seek(0, SeekOrigin.End);
                var headerReader = new ArchiveHeaderReverseReader(fileStream);
                var header = headerReader.ReadHeader();

                var splitInfo = new FileSplitInfo(header.ChunkCount);
                for (long id = 0, position = 0; id < splitInfo.ChunkCount; id++)
                {
                    var fileChunkInfo = new FileChunkInfo(id, header.ChunkLengths[id])
                    {
                        Position = position
                    };
                    splitInfo.Chunks[id] = fileChunkInfo;
                    position += fileChunkInfo.Length;
                }

                return splitInfo;
            }
        }
    }

}
