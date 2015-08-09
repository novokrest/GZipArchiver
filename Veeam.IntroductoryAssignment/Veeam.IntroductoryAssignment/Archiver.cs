using System;
using System.Threading;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileAssembling;
using Veeam.IntroductoryAssignment.FileConverting;
using Veeam.IntroductoryAssignment.FileSplitting;
using Veeam.IntroductoryAssignment.Tasks;
using Veeam.IntroductoryAssignment.ThreadPool;

namespace Veeam.IntroductoryAssignment
{
    internal interface IArchiver
    {
        void Compress(string originalFilePath, string archivePath);
        void Decompress(string archivePath, string unpackedFilePath);
    }

    class Archiver : IArchiver
    {
        private readonly ConvertTaskFactory _compressTaskFactory;
        private readonly ConvertTaskFactory _decompressTaskFactory;

        public Archiver(ConvertTaskFactory compressTaskFactory, ConvertTaskFactory decompressTaskFactory)
        {
            _compressTaskFactory = compressTaskFactory;
            _decompressTaskFactory = decompressTaskFactory;
        }

        public void Compress(string originalFilePath, string archivePath)
        {
            var taskPool = PriorityTaskPool.Instance;
            var fileSplitter = new RegularFileSplitter(originalFilePath);
            var fileAssembler = new ArchiveFileAssembler(archivePath, fileSplitter.GetChunkCount());
            var fileConverter = new FileConverter(new GZipCompressMemoryDataConverter(), fileAssembler, taskPool);
            foreach (var fileChunk in fileSplitter.GetFileChunks())
            {
                taskPool.AddTask(new ReadFileChunkTask(fileChunk, fileConverter), 0);
            }
            taskPool.Start();
            //taskPool.AddTask(new ExceptionTask(), 5);
            //taskPool.AddTask(new ExceptionTask(), 5);

            //fileAssembler.WaitForComplete();
            taskPool.WaitForCompleting();
            if (taskPool.Exception != null)
            {
                throw taskPool.Exception;
            }

            new object();
        }

        public void Decompress(string archivePath, string unpackedFilePath)
        {
            var taskPool = PriorityTaskPool.Instance;
            var fileSplitter = new ArchiveFileSplitter(archivePath);
            var fileAssembler = new RegularFileAssembler(unpackedFilePath, fileSplitter.GetChunkCount());
            var fileConverter = new FileConverter(new GZipDecompressMemoryDataConverter(), fileAssembler, taskPool);
            foreach (var fileChunk in fileSplitter.GetFileChunks())
            {
                taskPool.AddTask(new ReadFileChunkTask(fileChunk, fileConverter), 0);
            }
            taskPool.Start();
            taskPool.WaitForCompleting();

            //fileAssembler.WaitForComplete();
            //Console.WriteLine("Assembling complete");

            //taskPool.Stop();
        }
    }

    class GZipArchiver : Archiver
    {
        private static readonly IArchiver instance = new GZipArchiver();

        private GZipArchiver() 
            : base(new GZipCompressTaskFactory(), new GZipDecompressTaskFactory())
        {
        }

        public static IArchiver Instance
        {
            get { return instance; }
        }
    }

    internal class ConcurrentFileConverter
    {
        private readonly ConvertTaskFactory _taskFactory;

        public ConcurrentFileConverter(ConvertTaskFactory taskFactory)
        {
            _taskFactory = taskFactory;
        }

        public void Convert(string originalFilePath, string convertedFilePath)
        {
            //var taskPool = PriorityTaskPool.Instance;
            //var fileSplitter = new FileSplitter(originalFilePath, new FixedSplitInfoExtractor(originalFilePath));
            //var fileAssembler = new ArchiveFileAssembler(convertedFilePath, fileSplitter.GetChunkCount());
            //foreach (var fileChunk in fileSplitter.GetFileChunks())
            //{
            //    taskPool.AddTask(_taskFactory.CreateTask(fileChunk, fileAssembler));
            //}
            
            //fileAssembler.WaitForComplete();
            //Console.WriteLine("Assembling complete");
            //taskPool.Stop();
        }
    }
}
