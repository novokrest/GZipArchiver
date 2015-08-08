using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Veeam.IntroductoryAssignment.FileContentManagers;
using Veeam.IntroductoryAssignment.Util;

namespace Veeam.IntroductoryAssignment.Tasks
{
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
