using System;
using Veeam.IntroductoryAssignment.Common;

namespace Veeam.IntroductoryAssignment.FileSplitting
{
    internal class FixedSplitInfoExtractor : BaseFileSplitInfoExtractor
    {
        private static readonly int DefaultMaxChunkLength = 100*1024;//*1024;

        private readonly int _maxChunkLength;

        public FixedSplitInfoExtractor(string fileName)
            : this(fileName, DefaultMaxChunkLength)
        {
        }

        public FixedSplitInfoExtractor(string fileName, int maxChunkLength)
            : base(fileName)
        {
            _maxChunkLength = maxChunkLength;
        }

        public override FileSplitInfo GetFileSplitInfo()
        {
            var fileSize = GetFileSize();
            var chunkCount = fileSize%_maxChunkLength == 0 ? fileSize/_maxChunkLength : fileSize/_maxChunkLength + 1;
            var splitInfo = new FileSplitInfo(chunkCount);
            for (long id = 0, position = 0; id < splitInfo.ChunkCount; id++, position += _maxChunkLength)
            {
                splitInfo.Chunks[id] = new FileChunkInfo(id, (int)Math.Min(_maxChunkLength, fileSize - position))
                {
                    Position = position
                };
            }

            return splitInfo;
        }
    }
}
