using System.IO;

namespace Veeam.IntroductoryAssignment.Util
{
    static class FileStreamHelper
    {
        public static void ReadAll(this FileStream fileStream, byte[] buffer, int offset, int count)
        {
            int readCount = 0;
            while (readCount < count)
            {
                readCount += fileStream.Read(buffer, offset + readCount, count - readCount);
            }
        }
    }
}
