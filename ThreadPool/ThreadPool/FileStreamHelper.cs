using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Veeam.TestTask
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
