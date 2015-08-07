using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;

namespace Tester
{
    class ArchiveTester
    {
        private readonly string _originalFilePath;
        private readonly string _archiveFilePath;
        private readonly string _unpackedFilePath;

        public ArchiveTester(string archiverProgramPath, string testFilePath)
        {
            ArchiverProgramPath = archiverProgramPath;
            _originalFilePath = testFilePath;
            _archiveFilePath = _originalFilePath + ".gz";
            _unpackedFilePath = _archiveFilePath + ".unpacked";
        }

        public string ArchiverProgramPath { get; }

        public void Test()
        {
            CompressTest();
            DecompressTest();
            CheckFilesEquivalence();
        }

        private void CheckFilesEquivalence()
        {
            var process = Process.Start("fc", $"/B {_originalFilePath} {_unpackedFilePath}");
            Debug.Assert(process != null, "fc not started");
            process.WaitForExit();
            Debug.Assert(process.ExitCode == 0);

        }

        private void CompressTest()
        {
            RunProgram($"compress {_originalFilePath} {_archiveFilePath}", 0);
        }

        private void DecompressTest()
        {
            RunProgram($"compress {_archiveFilePath} {_unpackedFilePath}", 0);
        }

        public void RunProgram(string arguments, int expectedExitCode)
        {
            var process = Process.Start(ArchiverProgramPath, arguments);
            Debug.Assert(process != null, "archiver not started");
            process.WaitForExit();
            Debug.Assert(process.ExitCode == expectedExitCode);
        }
    }
}
