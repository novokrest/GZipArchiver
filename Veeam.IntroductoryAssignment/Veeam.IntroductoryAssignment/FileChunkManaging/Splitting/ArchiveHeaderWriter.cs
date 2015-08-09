using System.IO;

namespace Veeam.IntroductoryAssignment.FileChunkManaging.Splitting
{
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

}
