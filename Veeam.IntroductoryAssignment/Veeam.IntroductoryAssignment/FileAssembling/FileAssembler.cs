using System.Collections.Generic;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileSplitting;
using Veeam.IntroductoryAssignment.Tasks;
using Veeam.IntroductoryAssignment.ThreadPool;

namespace Veeam.IntroductoryAssignment.FileAssembling
{
    abstract class FileAssembler : BaseCompletionWaitable, ITaskCompletionObserver
    {
        private readonly string _fileName;
        private readonly FileSplitInfo _fileSplitInfo;
        private readonly FileDataHolder _fileDataHolder;
        private readonly ITaskPool _taskPool;

        private readonly HashSet<long> _assemblingChunks = new HashSet<long>();
        private int _assembledChunkCount;

        protected FileAssembler(string fileName, long chunkCount)
        {
            _fileName = fileName;
            _fileSplitInfo = new FileSplitInfo(chunkCount);
            _fileDataHolder = new FileDataHolder(fileName);
            _taskPool = PriorityTaskPool.Instance;
        }

        public FileSplitInfo SplitInfo
        {
            get { return _fileSplitInfo; }
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public void AddFileChunk(FileChunkInfo fileChunkInfo, byte[] data)
        {
            _fileDataHolder.SetData(fileChunkInfo, data);

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
            _taskPool.AddTask(CreateFileAssembleTask(chunkInfo), 2);
        }

        private FileAssembleTask CreateFileAssembleTask(FileChunkInfo fileChunkInfo)
        {
            var fileChunk = new FileChunk(FileName, fileChunkInfo, _fileDataHolder);
            return new FileAssembleTask(fileChunk, this);
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
            var chunks = SplitInfo.Chunks;
            if (chunks != null)
            {
                var prevChunkInfo = chunks[prevChunkId];
                if (prevChunkInfo.HasValidPosition())
                {
                    fileChunkInfo.Position = prevChunkInfo.Position + prevChunkInfo.Length;
                    return true;
                }
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

        protected abstract void CompleteAssembling();
    }
}
