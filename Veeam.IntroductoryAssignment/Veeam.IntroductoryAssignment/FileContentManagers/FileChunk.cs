using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Veeam.IntroductoryAssignment.FileContentManagers
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
            return String.Format("Id: {0}, Length: {1}, Position: {2}", Id, Length, Position);
        }
    }

    class FileChunk
    {
        private readonly string _fileName;
        private readonly FileChunkInfo _info;
        private readonly FileDataHolder _dataHolder;

        public FileChunk(string fileName, FileChunkInfo info, FileDataHolder dataHolder)
        {
            _fileName = fileName;
            _info = info;
            _dataHolder = dataHolder;
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

        public override string ToString()
        {
            return String.Format("FileName[{0}], Info[{1}]", FileName, Info);
        }
    }

}
