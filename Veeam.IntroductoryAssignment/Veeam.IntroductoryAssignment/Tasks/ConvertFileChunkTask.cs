using System;
using System.Diagnostics;
using System.Threading;
using Veeam.IntroductoryAssignment.FileContentManagers;
using Veeam.IntroductoryAssignment.Util;

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

        public IMemoryDataConverter Converter
        {
            get { return _converter; }
        }

        public FileChunk OriginalFileChunk
        {
            get { return _originalFileChunk; }
        }

        public FileAssembler ArchiveAssembler
        {
            get { return _archiveAssembler; }
        }

        public void Execute()
        {
            var data = OriginalFileChunk.GetData();
            var originalChunkInfo = OriginalFileChunk.Info;

            var convertedDataInfo = Converter.Convert(new DataInfo(originalChunkInfo.Length, data));
            OriginalFileChunk.DataHolder.ReleaseData(originalChunkInfo.Id);
            var convertedChunkInfo = new FileChunkInfo(originalChunkInfo.Id, convertedDataInfo.Length);
            //Debug.Assert(convertedDataInfo.Data[convertedDataInfo.Length - 4] == 1);

            ArchiveAssembler.AddFileChunk(convertedChunkInfo, convertedDataInfo.Data);
        }

        public override string ToString()
        {
            return String.Format("ConvertFileChunkTask: Converter={0}, FileChunk={1}", Converter, OriginalFileChunk);
        }
    }

    internal class VerboseTaskDecorator : ITask
    {
        private readonly ITask _task;

        public VerboseTaskDecorator(ITask task)
        {
            _task = task;
        }

        public void Execute()
        {
            Console.WriteLine("Task on thread {0} is started", Thread.CurrentThread.ManagedThreadId);
            _task.Execute();
            Console.WriteLine("Task on thread {0} has been executed", Thread.CurrentThread.ManagedThreadId);
        }

        public override string ToString()
        {
            return _task.ToString();
        }
    }
}
