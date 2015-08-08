﻿using System.IO;
using System.IO.Compression;

namespace Veeam.IntroductoryAssignment.Util
{
    internal class DataInfo
    {
        private readonly int _length;
        private readonly byte[] _data;

        public DataInfo(int length)
            : this(length, new byte[length])
        {
        }

        public DataInfo(int length, byte[] data)
        {
            _length = length;
            _data = data;
        }

        public int Length
        {
            get { return _length; }
        }

        public byte[] Data
        {
            get { return _data; }
        }
    }

    interface IMemoryDataConverter
    {
        DataInfo Convert(DataInfo dataInfo);
    }

    internal abstract class MemoryDataConverter : IMemoryDataConverter
    {
        public abstract DataInfo Convert(DataInfo dataInfo);
        protected abstract Stream GetConvertingStream(MemoryStream stream);
    }

    internal class GZipCompressMemoryDataConverter : IMemoryDataConverter
    {
        public DataInfo Convert(DataInfo dataInfo)
        {
            using (var memoryStream = new MemoryStream(dataInfo.Length))
            {
                using (var convertingstream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    convertingstream.Write(dataInfo.Data, 0, dataInfo.Length);
                }
                return new DataInfo((int)memoryStream.Position, memoryStream.GetBuffer());
            }
        }
    }

    internal class GZipDecompressMemoryDataConverter : IMemoryDataConverter
    {
        public DataInfo Convert(DataInfo dataInfo)
        {
            using (var memoryStream = new MemoryStream(dataInfo.Data, 0, dataInfo.Length))
            {
                var convertedDataInfo = new DataInfo(GetDecompressedSize(dataInfo));
                using (var convertingstream = new GZipStream(memoryStream, CompressionMode.Decompress, true))
                {
                    convertingstream.Read(convertedDataInfo.Data, 0, convertedDataInfo.Length);
                }
                return convertedDataInfo;
            }
        }

        private int GetDecompressedSize(DataInfo dataInfo)
        {
            using (var memoryStream = new MemoryStream(dataInfo.Data, dataInfo.Length - 4, 4))
            {
                var binaryReader = new BinaryReader(memoryStream);
                return binaryReader.ReadInt32();
            }
        }
    }
}
