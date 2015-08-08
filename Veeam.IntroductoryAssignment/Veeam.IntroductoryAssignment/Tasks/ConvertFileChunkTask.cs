using System;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileAssembling;

namespace Veeam.IntroductoryAssignment.Tasks
{
    internal class ConvertFileChunkTask : ITask
    {
        private readonly IMemoryDataConverter _converter;
        private readonly FileChunk _originalFileChunk;
        private readonly FileAssembler _archiveAssembler;

        public ConvertFileChunkTask(IMemoryDataConverter converter, FileChunk originalFileChunk, FileAssembler archiveAssembler)
        {
            _converter = converter;
            _originalFileChunk = originalFileChunk;
            _archiveAssembler = archiveAssembler;
        }

        public void Execute()
        {
            var originalChunkInfo = _originalFileChunk.Info;
            var originalData = _originalFileChunk.GetData();

            var convertedDataInfo = _converter.Convert(new DataInfo(originalChunkInfo.Length, originalData));
            _originalFileChunk.ReleaseData();

            var convertedChunkInfo = new FileChunkInfo(originalChunkInfo.Id, convertedDataInfo.Length);
            _archiveAssembler.AddFileChunk(convertedChunkInfo, convertedDataInfo.Data);
        }

        public override string ToString()
        {
            return String.Format("ConvertFileChunkTask[ {0}, {1} ]", _converter, _originalFileChunk);
        }
    }
}
