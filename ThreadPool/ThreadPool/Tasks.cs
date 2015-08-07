using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ThreadPool;

namespace Veeam.TestTask
{
    interface ITask
    {
        void Execute();
    }


    internal class ConvertFileChunkTask : ITask
    {
        public ConvertFileChunkTask(MemoryDataConverter converter, FileChunk originalFileChunk, FileAssembler archiveAssembler)
        {
            Converter = converter;
            OriginalFileChunk = originalFileChunk;
            ArchiveAssembler = archiveAssembler;
        }

        public FileChunk OriginalFileChunk { get; }
        public FileAssembler ArchiveAssembler { get; }
        public MemoryDataConverter Converter { get; }

        public void Execute()
        {
            var data = OriginalFileChunk.GetData();
            var originalChunkInfo = OriginalFileChunk.Info;

            var convertedDataInfo = Converter.Convert(new DataInfo(data, originalChunkInfo.Length));
            var convertedChunkInfo = new FileChunkInfo(originalChunkInfo.Id, convertedDataInfo.Length);

            ArchiveAssembler.AddFileChunk(convertedChunkInfo, convertedDataInfo.Data);
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
            return new ConvertFileChunkTask(new GZipCompressMemoryDataConverter(), fileChunk, fileAssembler);
        }
    }

    class GZipDecompressTaskFactory : ConvertTaskFactory
    {
        public override ITask CreateTask(FileChunk fileChunk, FileAssembler fileAssembler)
        {
            return new ConvertFileChunkTask(new GZipDecompressMemoryDataConverter(), fileChunk, fileAssembler);
        }
    }
}
