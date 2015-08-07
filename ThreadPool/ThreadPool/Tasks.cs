using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ThreadPool;

namespace Veeam.TestTask
{
    interface ITask
    {
        void Execute();
    }


    internal class ConvertFileChunkTask : ITask
    {
        public ConvertFileChunkTask(IMemoryDataConverter converter, FileChunk originalFileChunk, FileAssembler archiveAssembler)
        {
            Converter = converter;
            OriginalFileChunk = originalFileChunk;
            ArchiveAssembler = archiveAssembler;
        }

        public FileChunk OriginalFileChunk { get; }
        public FileAssembler ArchiveAssembler { get; }
        public IMemoryDataConverter Converter { get; }

        public void Execute()
        {
            var data = OriginalFileChunk.GetData();
            var originalChunkInfo = OriginalFileChunk.Info;

            var convertedDataInfo = Converter.Convert(new DataInfo(data, originalChunkInfo.Length));
            var convertedChunkInfo = new FileChunkInfo(originalChunkInfo.Id, convertedDataInfo.Length);

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
            Console.WriteLine($"Task on thread {Thread.CurrentThread.ManagedThreadId} is started");
            _task.Execute();
            Console.WriteLine($"Task on thread {Thread.CurrentThread.ManagedThreadId} has been executed");
        }

        public override string ToString()
        {
            return _task.ToString();
        }
    }

    internal abstract class ConvertTaskFactory
    {
        public abstract ITask CreateTask(FileChunk fileChunk, FileAssembler fileAssembler);
    }

    internal class GZipCompressTaskFactory : ConvertTaskFactory
    {
        public override ITask CreateTask(FileChunk fileChunk, FileAssembler fileAssembler)
        {
            return new VerboseTaskDecorator(new ConvertFileChunkTask(new GZipCompressMemoryDataConverter(), fileChunk, fileAssembler));
        }
    }

    class GZipDecompressTaskFactory : ConvertTaskFactory
    {
        public override ITask CreateTask(FileChunk fileChunk, FileAssembler fileAssembler)
        {
            return new VerboseTaskDecorator(new ConvertFileChunkTask(new GZipDecompressMemoryDataConverter(), fileChunk, fileAssembler));
        }
    }
}
