using System;
using System.IO;

namespace Tester
{
    class Program
    {
        static void Main()
        {
            var solutionDirPath =
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(Environment.CurrentDirectory)));

            var gzipBinSolutionRelativePath = @"Veeam.IntroductoryAssignment\bin\Debug\GZipTest.exe";
            var programPath = solutionDirPath + Path.DirectorySeparatorChar + gzipBinSolutionRelativePath;

            var smallFileName = "small.pdf";
            var smallArchiveName = smallFileName + ".gz";
            var smallUnpackedName = "unpacked_" + smallFileName;

            var bigFileName = Path.GetFullPath("batman.pdf");
            var bigArchiveName = bigFileName + ".gz";
            var bigUnpackedName = "unpacked_" + bigFileName;

            var nonExistentFileName = "nullfile";

            try
            {
                new GZipCompressTester(programPath, smallFileName, smallArchiveName, 0)
                {
                    Description = "Test COMPRESS on small file. Archive doesn't exist."
                }
                    .Test();

                new GZipDecompressTester(programPath, smallArchiveName, smallUnpackedName, 0)
                {
                    Description = "Test DECOMPRESS on small file. Unpacked doesn't exist."
                }
                    .Test();

                new GZipCompressTester(programPath, smallFileName, smallArchiveName, 0)
                {
                    Description = "Test COMPRESS on small file. Archive already exists"
                }
                    .Test();

                new GZipDecompressTester(programPath, smallArchiveName, smallUnpackedName, 0)
                {
                    Description = "Test DECOMPRESS on small file. Unpacked already exists"
                }
                    .Test();

                new GZipCompressTester(programPath, nonExistentFileName, nonExistentFileName,
                    1)
                {
                    Description = "Test COMPRESS on nonexistent file."
                }
                    .Test();

                new GZipDecompressTester(programPath, nonExistentFileName, nonExistentFileName,
                    1)
                {
                    Description = "Test DECOMPRESS on nonexistent file."
                }
                    .Test();

                new GZipCompressTester(programPath, smallFileName, smallFileName, 1)
                {
                    Description = "Test COMPRESS on 'myself'"
                }
                    .Test();

                new GZipDecompressTester(programPath, smallArchiveName, smallArchiveName, 1)
                {
                    Description = "Test DECOMPRESS on 'myself'"
                }
                    .Test();

                new GZipDecompressTester(programPath, smallFileName, smallUnpackedName,
                    1)
                {
                    Description = "Test DECOMPRESS on incorrect archive."
                }
                    //.Test()
                    ;

            new GZipCompressTester(programPath, bigFileName, bigArchiveName, 0)
            {
                Description = "Test COMPRESS on big file. Archive doesn't exist."
            }
            //.Test()
            ;

            new GZipDecompressTester(programPath, bigArchiveName, bigUnpackedName, 0)
            {
                Description = "Test DECOMPRESS on big file. Unpacked doesn't exist."
            }
            //.Test()
            ;

                Console.WriteLine("!!!SUCCESS!!!");
            }
            catch (Exception e)
            {
                Console.WriteLine("!!!!!FAIL!!!!!");
                return;
            }

            new object();
        }
    }
}
