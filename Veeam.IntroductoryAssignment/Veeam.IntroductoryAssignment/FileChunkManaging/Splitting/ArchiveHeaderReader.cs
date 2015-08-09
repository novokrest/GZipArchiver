using System.IO;

namespace Veeam.IntroductoryAssignment.FileChunkManaging.Splitting
{
    interface IArchiveHeaderReader
    {
        ArchiveHeader ReadHeader();
    }

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
}
