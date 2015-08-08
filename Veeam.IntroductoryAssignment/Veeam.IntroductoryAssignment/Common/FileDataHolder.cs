using System.Collections.Generic;

namespace Veeam.IntroductoryAssignment.Common
{
    class FileDataHolder
    {
        private readonly string _fileName;
        private readonly Dictionary<long, byte[]> _chunkDatas = new Dictionary<long, byte[]>();

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
            return _chunkDatas[chunkInfo.Id];    
        }

        public bool HasData(FileChunkInfo chunkInfo)
        {
            return _chunkDatas.ContainsKey(chunkInfo.Id);
        }

        public void ReleaseData(FileChunkInfo chunkInfo)
        {
            _chunkDatas[chunkInfo.Id] = null;
        }
    }
}
