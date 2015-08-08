using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veeam.IntroductoryAssignment.FileContentManagers
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

        public void ReleaseData(long id)
        {
            _chunkDatas[id] = null;
        }
    }
}
