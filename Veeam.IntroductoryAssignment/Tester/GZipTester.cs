using System;
using System.Diagnostics;

namespace Tester
{
    abstract class ProgramTester
    {
        private readonly string _programPath;
        private readonly int _expectedExitCode;

        protected ProgramTester(string programPath, int expectedExitCode)
        {
            _programPath = programPath;
            _expectedExitCode = expectedExitCode;
        }

        public string ProgramPath
        {
            get { return _programPath; }
        }

        public int ExpectedExitCode
        {
            get { return _expectedExitCode; }
        }

        protected void RunProgram(string arguments)
        {
            Console.WriteLine("Start {0} with arguments '{1}'", ProgramPath, arguments);
            var startInfo = new ProcessStartInfo(ProgramPath, arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new ProgramTesterException("Process is null");
            }
            //process.BeginOutputReadLine();
            process.WaitForExit();
            Console.Write("Process output: {0}", process.StandardOutput.ReadToEnd());
            if (process.ExitCode != ExpectedExitCode)
            {
                throw new ProgramTesterException(String.Format("Unexpected exit code: expected={0}, actual={1}", ExpectedExitCode, process.ExitCode));
            }
        }

        public abstract void Test();
    }

    abstract class GZipTester : ProgramTester
    {
        private readonly string _originalFileName;
        private readonly string _convertedFileName;

        public string Description { get; set; }

        public string OriginalFileName
        {
            get { return _originalFileName; }
        }

        public string ConvertedFileName
        {
            get { return _convertedFileName; }
        }

        protected GZipTester(string programPath, string originalFileName, string convertedFileName, int compressExitCode)
            : base(programPath, compressExitCode)
        {
            _originalFileName = originalFileName;
            _convertedFileName = convertedFileName;
        }

        public override void Test()
        {
            Console.WriteLine(Description);
            try
            {
                RunProgram(GetArguments());
                RunProgram(GetArguments());
                Console.WriteLine("Test SUCCESS");
            }
            catch (Exception e)
            {
                Console.WriteLine("Test FAILED: " + e.Message);
                throw;
            }
            Console.WriteLine("Test completed");
            Console.WriteLine();
        }

        protected abstract string GetArguments();
    }

    class GZipDecompressTester : GZipTester
    {
        public GZipDecompressTester(string programPath, string originalFileName, string convertedFileName, int compressExitCode)
            : base(programPath, originalFileName, convertedFileName, compressExitCode)
        {
        }

        protected override string GetArguments()
        {
            return String.Format("decompress {0} {1}", OriginalFileName, ConvertedFileName);
        }
    }

    class GZipCompressTester : GZipTester
    {
        public GZipCompressTester(string programPath, string originalFileName, string convertedFileName, int compressExitCode)
            : base(programPath, originalFileName, convertedFileName, compressExitCode)
        {
        }

        protected override string GetArguments()
        {
            return String.Format("compress {0} {1}", OriginalFileName, ConvertedFileName);
        }
    }

    internal class ProgramTesterException : Exception
    {
        public ProgramTesterException(string message)
            : base(message)
        {
        }

        public ProgramTesterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
