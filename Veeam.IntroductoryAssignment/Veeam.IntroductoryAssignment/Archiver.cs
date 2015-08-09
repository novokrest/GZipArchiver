using Veeam.IntroductoryAssignment.FileChunkManaging;
using Veeam.IntroductoryAssignment.FileConverting;
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
        private readonly IMemoryDataConverter _compressDataConverter;
        private readonly IMemoryDataConverter _decompressDataConverter;

        public Archiver(IMemoryDataConverter compressDataConverter, IMemoryDataConverter decompressDataConverter)
        {
            _compressDataConverter = compressDataConverter;
            _decompressDataConverter = decompressDataConverter;
        }

        public void Compress(string originalFilePath, string archivePath)
        {
            var fileSplitter = new RegularFileSplitter(originalFilePath);
            var fileAssembler = new ArchiveFileAssembler(archivePath, fileSplitter.GetChunkCount());
            Transform(fileSplitter, fileAssembler, _compressDataConverter);
        }

        public void Decompress(string archivePath, string unpackedFilePath)
        {
            var fileSplitter = new ArchiveFileSplitter(archivePath);
            var fileAssembler = new RegularFileAssembler(unpackedFilePath, fileSplitter.GetChunkCount());
            Transform(fileSplitter, fileAssembler, _decompressDataConverter);
        }

        private void Transform(FileSplitter originalFileSplitter, FileAssembler convertedFileAssembler, IMemoryDataConverter dataConverter)
        {
            var taskPool = PriorityTaskPool.Instance;
            var fileConverter = new FileConverter(dataConverter, convertedFileAssembler, taskPool);

            foreach (var fileChunk in originalFileSplitter.GetFileChunks())
            {
                taskPool.AddTask(new ReadFileChunkTask(fileChunk, fileConverter), 0);
            }
            taskPool.Start();
            taskPool.WaitForCompleting();
            if (taskPool.Exception != null)
            {
                throw taskPool.Exception;
            }
        }
    }

    class GZipArchiver : Archiver
    {
        public GZipArchiver() 
            : base(new GZipCompressMemoryDataConverter(), new GZipDecompressMemoryDataConverter())
        {
        }
    }
}
