using System;
using Veeam.IntroductoryAssignment.FileDataManaging;

namespace Veeam.IntroductoryAssignment.FileChunkManaging
{
    internal class FileChunkInfo
    {
        private static readonly long InvalidPosition = -1;
        
        private readonly long _id;
        private readonly int _length;

        public FileChunkInfo(long id, int length)
        {
            _id = id;
            _length = length;
            Position = InvalidPosition;
        }

        public long Id
        {
            get { return _id; }
        }

        public int Length
        {
            get { return _length; }
        }

        public long Position { get; set; }

        public bool HasValidPosition()
        {
            return Position > InvalidPosition;
        }

        public void SetInvalidPosition()
        {
            Position = InvalidPosition;
        }

        public override string ToString()
        {
            return String.Format("ChunkInfo[ Id: {0}, Length: {1}, Position: {2} ]", Id, Length, Position);
        }
    }

    class FileChunk
    {
        private readonly string _fileName;
        private readonly FileChunkInfo _info;
        private readonly FileDataHolder _dataHolder;
        private readonly FileDataReader _dataReader;
        private readonly FileDataWriter _dataWriter;

        public FileChunk(string fileName, FileChunkInfo info, FileDataHolder dataHolder, FileDataReader dataReader, FileDataWriter dataWriter)
        {
            _fileName = fileName;
            _info = info;
            _dataHolder = dataHolder;
            _dataReader = dataReader;
            _dataWriter = dataWriter;
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public FileChunkInfo Info
        {
            get { return _info; }
        }

        public FileDataHolder DataHolder
        {
            get { return _dataHolder; }
        }

        public byte[] GetData()
        {
            return DataHolder.GetData(Info);
        }

        public void SetData(byte[] data)
        {
            DataHolder.SetData(Info, data);
        }

        public void ReadData()
        {
            _dataReader.ReadData(Info, GetData());
        }

        public void WriteData()
        {
            _dataWriter.WriteData(Info, GetData());
        }

        public void ReleaseData()
        {
            DataHolder.ReleaseData(Info);
        }

        public override string ToString()
        {
            return String.Format("FileChunk[ Filename: {0}, {1} ]", FileName, Info);
        }
    }

}
