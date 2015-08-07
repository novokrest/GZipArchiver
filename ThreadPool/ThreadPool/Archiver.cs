using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Veeam.TestTask
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
            var converter = new ConcurrentFileConverter(_compressTaskFactory);
            converter.Convert(originalFilePath, archivePath);
        }

        public void Decompress(string archivePath, string unpackedFilePath)
        {
            var converter = new ConcurrentFileConverter(_decompressTaskFactory);
            converter.Convert(archivePath, unpackedFilePath);
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
            var threadPool = PriorityTaskPool.Instance;
            var fileSplitter = new FileSplitter(originalFilePath);
            var fileAssembler = new FileAssembler(convertedFilePath);
            foreach (var fileChunk in fileSplitter.GetFileChunks())
            {
                threadPool.AddTask(_taskFactory.CreateTask(fileChunk, fileAssembler));
            }
        }
    }
}
