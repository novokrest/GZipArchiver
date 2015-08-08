using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileAssembling;
using Veeam.IntroductoryAssignment.Tasks.Decorators;

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
            return new VerboseTask(new ConvertFileChunkTask(new GZipCompressMemoryDataConverter(), fileChunk, fileAssembler));
        }
    }

    class GZipDecompressTaskFactory : ConvertTaskFactory
    {
        public override ITask CreateTask(FileChunk fileChunk, FileAssembler fileAssembler)
        {
            return new VerboseTask(new ConvertFileChunkTask(new GZipDecompressMemoryDataConverter(), fileChunk, fileAssembler));
        }
    }
}
