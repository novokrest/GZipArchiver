using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Veeam.IntroductoryAssignment.Tasks;
using Veeam.IntroductoryAssignment.ThreadPool;

namespace Veeam.IntroductoryAssignment.FileContentManagers
{
    abstract class FileAssembler
    {
        private readonly string _fileName;
        private readonly FileSplitInfo _fileSplitInfo;
        private readonly FileDataHolder _fileDataHolder;
        private readonly ITaskPool _taskPool;
        private readonly HashSet<long> _assembledChunks = new HashSet<long>();

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
            _fileDataHolder.AddData(fileChunkInfo, data);

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
            return chunkInfo != null && TryComputePosition(chunkInfo) && !_assembledChunks.Contains(chunkInfo.Id);
        }

        private void AssembleChunk(FileChunkInfo chunkInfo)
        {
            _taskPool.AddTask(CreateFileAssembleTask(chunkInfo), 1);
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

        private readonly Dictionary<Thread, AutoResetEvent> _waiters = new Dictionary<Thread, AutoResetEvent>();

        public void WaitForComplete()
        {
            var thread = Thread.CurrentThread;
            var waitHandler = new AutoResetEvent(false);
            _waiters.Add(thread, waitHandler);
            waitHandler.WaitOne();;
        }

        protected abstract void CompleteAssembling();

        public void NotifyAboutAssemblingCompletion()
        {
            foreach (var waitHandler in _waiters.Values)
            {
                waitHandler.Set();
            }
        }

        public void NotifyAboutTaskCompletion(FileChunk fileChunk)
        {
            _fileDataHolder.ReleaseData(fileChunk.Info.Id);
            AddAssembledChunk(fileChunk.Info);
        }

        private void AddAssembledChunk(FileChunkInfo info)
        {
            _assembledChunks.Add(info.Id);
            if (_assembledChunks.Count == SplitInfo.ChunkCount)
            {
                CompleteAssembling();
                NotifyAboutAssemblingCompletion();
            }
        }
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
         
        }
    }

}
