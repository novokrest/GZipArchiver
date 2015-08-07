using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Veeam.TestTask
{
    internal class FileChunkInfo
    {
        private static readonly long InvalidPosition = -1;

        public FileChunkInfo(long id, int length)
        {
            Id = id;
            Length = length;
            Position = InvalidPosition;
        }

        public long Id { get; }
        public int Length { get; }
        public long Position { get; set; }

        public bool HasValidPosition()
        {
            return Position > InvalidPosition;
        }

        public void SetInvalidPosition()
        {
            Position = InvalidPosition;
        }
    }

    class FileChunk
    {
        public FileChunk(string fileName, FileChunkInfo info, FileDataHolder dataHolder)
        {
            DataHolder = dataHolder;
            FileName = fileName;
            Info = info;
        }

        public string FileName { get; }
        public FileChunkInfo Info { get; }
        public FileDataHolder DataHolder { get; }

        public byte[] GetData()
        {
            return DataHolder.GetData(Info);
        }
    }

    class FileDataHolder
    {
        private readonly Dictionary<long, byte[]> _chunkDatas = new Dictionary<long, byte[]>();

        public FileDataHolder(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; }

        public void AddData(FileChunkInfo chunkInfo, byte[] data)
        {
            _chunkDatas.Add(chunkInfo.Id, data);
        }

        public virtual byte[] GetData(FileChunkInfo chunkInfo)
        {
            return _chunkDatas[chunkInfo.Id];
        }

        public bool HasData(FileChunkInfo chunkInfo)
        {
            return _chunkDatas.ContainsKey(chunkInfo.Id);
        }
    }

    class FileSplitter : FileDataHolder
    {
        private FileChunkInfo[] _chunkInfos;

        public FileSplitter(string fileName) 
            : base(fileName)
        {
            Split(GetMagicChunkLength());
        }

        public override byte[] GetData(FileChunkInfo chunkInfo)
        {
            if (!HasData(chunkInfo))
            {
                ReadData(chunkInfo);
            }
            return base.GetData(chunkInfo);
        }

        private void ReadData(FileChunkInfo chunkInfo)
        {
            var data = new byte[chunkInfo.Length];
            using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                fileStream.Seek(chunkInfo.Position, SeekOrigin.Begin);
                fileStream.ReadAll(data, 0, data.Length);
            }
            AddData(chunkInfo, data);
        }

        private void Split(long maxChunkLength)
        {
            var fileSize = GetFileSize();
            _chunkInfos = new FileChunkInfo[fileSize / maxChunkLength + 1];

            for (long position = 0, id = 0; position < fileSize; position += maxChunkLength, id++)
            {
                _chunkInfos[id] = new FileChunkInfo(id, (int) Math.Min(maxChunkLength, fileSize - position))
                {
                    Position = position
                };
            }
        }

        public IEnumerable<FileChunk> GetFileChunks()
        {
            var chunkSize = GetMagicChunkLength();
            return GetFileChunks(chunkSize);
        }

        private int GetMagicChunkLength()
        {
            const int magic = 100*1024*1024;

            return (int) Math.Min(magic, GetFileSize());
        }

        private long GetFileSize()
        {
            return new FileInfo(FileName).Length;
        }

        public IEnumerable<FileChunk> GetFileChunks(long maxChunkSize)
        {
            return _chunkInfos.Select(GetFileChunk);
        }

        private FileChunk GetFileChunk(FileChunkInfo chunkInfo)
        {
            return new FileChunk(FileName, chunkInfo, this);
        }
    }

    class FileAssembler : FileDataHolder
    {
        private readonly Dictionary<long, FileChunkInfo> _chunkInfos = new Dictionary<long, FileChunkInfo>();
        private readonly Queue<long> _readyChunks = new Queue<long>();

        private readonly ITaskPool _taskPool;

        public FileAssembler(string fileName)
            : this(fileName, PriorityTaskPool.Instance)
        {
        }

        public FileAssembler(string fileName, ITaskPool taskPool)
            : base(fileName)
        {
            _taskPool = taskPool;
        }

        public void AddFileChunk(FileChunkInfo fileChunkInfo, byte[] data)
        {
            AddData(fileChunkInfo, data);
            _chunkInfos.Add(fileChunkInfo.Id, fileChunkInfo);
            if (TryComputePosition(fileChunkInfo))
            {
                UpdateChunkPositions(fileChunkInfo);
                _taskPool.AddTask(CreateFileAssembleTask(fileChunkInfo));
            }
        }

        private void UpdateChunkPositions(FileChunkInfo fileChunkInfo)
        {
            var currentChunkId = fileChunkInfo.Id;
            var nextChunkId = currentChunkId + 1;
            while (_chunkInfos.ContainsKey(nextChunkId))
            {
                var currentChunkInfo = _chunkInfos[currentChunkId];
                var nextChunkInfo = _chunkInfos[nextChunkId];
                if (nextChunkInfo.HasValidPosition()) break;
                nextChunkInfo.Position = currentChunkInfo.Position + currentChunkInfo.Length;
                _taskPool.AddTask(CreateFileAssembleTask(nextChunkInfo));
                currentChunkId = nextChunkId;
                nextChunkId = currentChunkId + 1;
            }
        }

        private FileAssembleTask CreateFileAssembleTask(FileChunkInfo fileChunkInfo)
        {
            var fileChunk = new FileChunk(FileName, fileChunkInfo, this);
            return new FileAssembleTask(fileChunk);
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
            if (_chunkInfos.ContainsKey(prevChunkId))
            {
                var prevChunkInfo = _chunkInfos[prevChunkId];
                if (prevChunkInfo.HasValidPosition())
                {
                    fileChunkInfo.Position = prevChunkInfo.Position + prevChunkInfo.Length;
                    return true;
                }
            }
            fileChunkInfo.SetInvalidPosition();
            return false;
        }
    }

    class FileAssembleTask : ITask
    {
        private readonly FileChunk _fileChunk;

        public FileAssembleTask(FileChunk fileChunk)
        {
            _fileChunk = fileChunk;
        }

        public void Execute()
        {
            var fileName = _fileChunk.FileName;
            var chunkInfo = _fileChunk.Info;
            var data = _fileChunk.GetData();

            using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fileStream.Seek(chunkInfo.Position, SeekOrigin.Begin);
                fileStream.Write(data, 0, chunkInfo.Length);
            }
        }
    }
}
