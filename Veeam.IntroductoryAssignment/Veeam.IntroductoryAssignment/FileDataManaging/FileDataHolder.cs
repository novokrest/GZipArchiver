using System.Collections.Generic;
using System.IO;
using System.Net;
using Veeam.IntroductoryAssignment.FileDataManaging;

namespace Veeam.IntroductoryAssignment.Common
{
    class FileDataHolder : FileNameHolder
    {
        private readonly Dictionary<long, byte[]> _chunkDatas = new Dictionary<long, byte[]>();

        public FileDataHolder(string fileName) 
            : base(fileName)
        {
        }

        public virtual void SetData(FileChunkInfo chunkInfo, byte[] data)
        {
            _chunkDatas.Add(chunkInfo.Id, data);
        }

        public virtual byte[] GetData(FileChunkInfo chunkInfo)
        {
            var chunkId = chunkInfo.Id;
            if (!_chunkDatas.ContainsKey(chunkId))
            {
                var data = new byte[chunkInfo.Length];
                _chunkDatas.Add(chunkId, data);
            }
            return _chunkDatas[chunkId];

        }

        public virtual void ReleaseData(FileChunkInfo chunkInfo)
        {
            var chunkId = chunkInfo.Id;
            if (_chunkDatas.ContainsKey(chunkId))
            {
                _chunkDatas[chunkId] = null;
            }

        }
    }

    class ConcurrentFileDataHolder : FileDataHolder
    {
        private readonly object _lock = new object();

        public ConcurrentFileDataHolder(string fileName)
            : base(fileName)
        {
        }

        public override void SetData(FileChunkInfo chunkInfo, byte[] data)
        {
            lock (_lock)
            {
                base.SetData(chunkInfo, data);
            }
        }

        public override byte[] GetData(FileChunkInfo chunkInfo)
        {
            lock (_lock)
            {
                return base.GetData(chunkInfo);
            }
        }

        public override void ReleaseData(FileChunkInfo chunkInfo)
        {
            lock (_lock)
            {
                base.ReleaseData(chunkInfo);
            }
        }
    }
}
