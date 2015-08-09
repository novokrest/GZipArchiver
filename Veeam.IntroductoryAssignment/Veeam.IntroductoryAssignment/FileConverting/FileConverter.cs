using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileChunkManaging;
using Veeam.IntroductoryAssignment.Tasks;
using Veeam.IntroductoryAssignment.ThreadPool;

namespace Veeam.IntroductoryAssignment.FileConverting
{
    class FileConverter : ITaskCompletionObserver
    {
        private readonly IMemoryDataConverter _converter;
        private readonly FileAssembler _fileAssembler;
        private readonly ITaskConsumer _taskPool;

        public FileConverter(IMemoryDataConverter converter, FileAssembler fileAssembler, ITaskConsumer taskPool)
        {
            _converter = converter;
            _fileAssembler = fileAssembler;
            _taskPool = taskPool;
        }

        public void HandleTaskCompletion(FileChunk chunk)
        {
            _taskPool.AddTask(new ConvertFileChunkTask(_converter, chunk, _fileAssembler), 1);
        }
    }
}
