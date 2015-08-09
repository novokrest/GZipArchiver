namespace Veeam.IntroductoryAssignment.FileChunkManaging.Splitting
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
}
