using System.Collections.Generic;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileChunkManaging;

namespace Veeam.IntroductoryAssignment.FileSplitting
{
    interface IFileSplitter
    {
        IEnumerable<FileChunk> GetFileChunks();
    }

    internal abstract class FileSplitter : FileChunkProducer
    {
        protected FileSplitter(string fileName, IFileSplitInfoExtractor splitInfoExtractor)
            : base(fileName, splitInfoExtractor.GetFileSplitInfo(), new ConcurrentFileDataHolder(fileName), new ConcurrentFileDataReader(fileName),new ConcurrentFileDataWriter(fileName))
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
