namespace Veeam.IntroductoryAssignment.Common
{
    interface ITaskCompletionObserver
    {
        void NotifyAboutTaskCompletion(FileChunk chunk);
    }
}
