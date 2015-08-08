using System.Collections.Generic;

namespace Veeam.IntroductoryAssignment.Common
{
    class FileDataHolder
    {
        private readonly string _fileName;
        private readonly Dictionary<long, byte[]> _chunkDatas = new Dictionary<long, byte[]>();
        private readonly object _lock = new object();

        public FileDataHolder(string fileName)
        {
            _fileName = fileName;
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public void SetData(FileChunkInfo chunkInfo, byte[] data)
        {
            lock (_lock)
            {
                _chunkDatas.Add(chunkInfo.Id, data);    
            }
        }

        public virtual byte[] GetData(FileChunkInfo chunkInfo)
        {
            lock (_lock)
            {
                var chunkId = chunkInfo.Id;
                if (!_chunkDatas.ContainsKey(chunkId))
                {
                    var data = new byte[chunkInfo.Length];
                    _chunkDatas.Add(chunkId, data);
                }
                return _chunkDatas[chunkId];
            }
        }

        public void ReleaseData(FileChunkInfo chunkInfo)
        {
            lock (_lock)
            {
                var chunkId = chunkInfo.Id;
                if (_chunkDatas.ContainsKey(chunkId))
                {
                    _chunkDatas[chunkId] = null;
                }   
            }
        }
    }
}
