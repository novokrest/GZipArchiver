using Veeam.IntroductoryAssignment.FileChunkManaging;

namespace Veeam.IntroductoryAssignment.Common
{
    interface ITaskCompletionObserver
    {
        void HandleTaskCompletion(FileChunk chunk);
    }
}
