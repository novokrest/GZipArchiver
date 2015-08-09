using System.Collections.Generic;
using System.IO;
using Veeam.IntroductoryAssignment.Common;
using Veeam.IntroductoryAssignment.FileChunkManaging.Splitting;
using Veeam.IntroductoryAssignment.FileDataManaging;
using Veeam.IntroductoryAssignment.Tasks;
using Veeam.IntroductoryAssignment.ThreadPool;

namespace Veeam.IntroductoryAssignment.FileChunkManaging
{
    abstract class FileAssembler : FileChunkProducer, ITaskCompletionObserver
    {
        private readonly ITaskPool _taskPool;

        private readonly HashSet<long> _assemblingChunks = new HashSet<long>();
        private int _assembledChunkCount;

        protected FileAssembler(string fileName, long chunkCount)
            : base(fileName, new FileSplitInfo(chunkCount), new ConcurrentFileDataHolder(fileName), new FileDataReader(fileName), new FileDataWriter(fileName))
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

        public void HandleTaskCompletion(FileChunk chunk)
        {
            ++_assembledChunkCount;
            if (_assembledChunkCount == SplitInfo.ChunkCount)
            {
                CompleteAssembling();
            }
        }

        protected abstract void CompleteAssembling();
    }

    class ArchiveFileAssembler : FileAssembler
    {
        public ArchiveFileAssembler(string fileName, long chunkCount)
            : base(fileName, chunkCount)
        {
        }

        protected override void CompleteAssembling()
        {
            using (var fileStream = new FileStream(FileName, FileMode.Append, FileAccess.Write))
            {
                var headerWriter = new ArchiveHeaderWriter(fileStream);
                headerWriter.WriteHeader(ArchiveHeader.Create(SplitInfo));
            }
            TaskPool.Stop();
        }
    }

    class RegularFileAssembler : FileAssembler
    {
        public RegularFileAssembler(string fileName, long chunkCount)
            : base(fileName, chunkCount)
        {
        }

        protected override void CompleteAssembling()
        {
            TaskPool.Stop();
        }
    }
}
