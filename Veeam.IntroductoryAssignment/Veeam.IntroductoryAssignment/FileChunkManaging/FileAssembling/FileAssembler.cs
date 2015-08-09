using System.Collections.Generic;
using System.IO;
using System.Threading;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileChunkManaging;
using Veeam.IntroductoryAssignment.FileSplitting;
using Veeam.IntroductoryAssignment.Tasks;
using Veeam.IntroductoryAssignment.ThreadPool;

namespace Veeam.IntroductoryAssignment.FileAssembling
{
    abstract class FileAssembler : FileChunkProducer, ITaskCompletionObserver
    {
        private readonly ITaskPool _taskPool;

        private readonly HashSet<long> _assemblingChunks = new HashSet<long>();
        private int _assembledChunkCount;

        protected FileAssembler(string fileName, long chunkCount)
            : base(fileName, new FileSplitInfo(chunkCount), new ConcurrentFileDataHolder(fileName),new ConcurrentFileDataReader(fileName), new ConcurrentFileDataWriter(fileName))
        {
            _taskPool = PriorityTaskPool.Instance;
        }

        public ITaskPool TaskPool
        {
            get { return _taskPool; }
        }

        public void AddFileChunk(FileChunkInfo fileChunkInfo, byte[] data)
        {
            DataHolder.SetData(fileChunkInfo, data);

            var chunkId = fileChunkInfo.Id;
            SplitInfo.Chunks[chunkId] = fileChunkInfo;
            UpdateChunkPositions(chunkId);
        }

        private void UpdateChunkPositions(long startChunkId)
        {
            for (var id = startChunkId; id < SplitInfo.ChunkCount; id++)
            {
                var chunkInfo = SplitInfo.Chunks[id];
                if (!CanAssembleChunk(chunkInfo)) break;
                AssembleChunk(chunkInfo);
            }
        }

        private bool CanAssembleChunk(FileChunkInfo chunkInfo)
        {
            return chunkInfo != null && TryComputePosition(chunkInfo) && !_assemblingChunks.Contains(chunkInfo.Id);
        }

        private void AssembleChunk(FileChunkInfo chunkInfo)
        {
            _assemblingChunks.Add(chunkInfo.Id);
            TaskPool.AddTask(CreateFileAssembleTask(chunkInfo), 2);
        }

        private FileAssembleTask CreateFileAssembleTask(FileChunkInfo fileChunkInfo)
        {
            return new FileAssembleTask(CreateFileChunk(fileChunkInfo), this);
        }

        private bool TryComputePosition(FileChunkInfo fileChunkInfo)
        {
            var chunkId = fileChunkInfo.Id;
            if (chunkId == 0)
            {
                fileChunkInfo.Position = 0;
                return true;
            }
            var prevChunkId = chunkId - 1;
            var prevChunkInfo = SplitInfo.Chunks[prevChunkId];
            if (prevChunkInfo != null && prevChunkInfo.HasValidPosition())
            {
                fileChunkInfo.Position = prevChunkInfo.Position + prevChunkInfo.Length;
                return true;
            }

            fileChunkInfo.SetInvalidPosition();
            return false;
        }

        public void NotifyAboutTaskCompletion(FileChunk chunk)
        {
            ++_assembledChunkCount;
            if (_assembledChunkCount == SplitInfo.ChunkCount)
            {
                CompleteAssembling();
                NotifyWaiters();
            }
        }


        private readonly Dictionary<Thread, AutoResetEvent> _waiters = new Dictionary<Thread, AutoResetEvent>();

        public void WaitForComplete()
        {
            var thread = Thread.CurrentThread;
            var waitHandler = new AutoResetEvent(false);
            _waiters.Add(thread, waitHandler);
            waitHandler.WaitOne();
        }

        public void NotifyWaiters()
        {
            foreach (var waitHandler in _waiters.Values)
            {
                waitHandler.Set();
            }
        }


        protected abstract void CompleteAssembling();
    }
}
