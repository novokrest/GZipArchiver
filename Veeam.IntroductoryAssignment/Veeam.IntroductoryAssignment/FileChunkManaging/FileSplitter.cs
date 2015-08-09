using System.Collections.Generic;
using Veeam.IntroductoryAssignment.FileChunkManaging.Splitting;
using Veeam.IntroductoryAssignment.FileDataManaging;

namespace Veeam.IntroductoryAssignment.FileChunkManaging
{
    interface IFileSplitter
    {
        IEnumerable<FileChunk> GetFileChunks();
    }

    internal abstract class FileSplitter : FileChunkProducer
    {
        protected FileSplitter(string fileName, IFileSplitInfoExtractor splitInfoExtractor)
            : base(fileName, splitInfoExtractor.GetFileSplitInfo(), new ConcurrentFileDataHolder(fileName), new FileDataReader(fileName),new FileDataWriter(fileName))
        {
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
