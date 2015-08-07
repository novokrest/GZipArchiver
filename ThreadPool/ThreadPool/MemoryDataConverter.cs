using System.IO;
using System.IO.Compression;

namespace ThreadPool
{
    internal class DataInfo
    {
        public DataInfo(byte[] data, int length)
        {
            Data = data;
            Length = length;
        }

        public byte[] Data { get; }
        public int Length { get; }
    }

    internal abstract class MemoryDataConverter
    {
        public DataInfo Convert(DataInfo dataInfo)
        {
            using (var memoryStream = new MemoryStream(dataInfo.Length))
            {
                using (var convertingstream = GetConvertingStream(memoryStream))
                {
                    convertingstream.Write(dataInfo.Data, 0, dataInfo.Length);
                }
                return new DataInfo(memoryStream.GetBuffer(), (int)memoryStream.Position);
            }
        }

        protected abstract Stream GetConvertingStream(MemoryStream stream);
    }

    internal class GZipCompressMemoryDataConverter : MemoryDataConverter
    {
        protected override Stream GetConvertingStream(MemoryStream stream)
        {
            return new GZipStream(stream, CompressionMode.Compress, true);
        }
    }

    internal class GZipDecompressMemoryDataConverter : MemoryDataConverter
    {
        protected override Stream GetConvertingStream(MemoryStream stream)
        {
            return new GZipStream(stream, CompressionMode.Decompress, true);
        }
    }
}
