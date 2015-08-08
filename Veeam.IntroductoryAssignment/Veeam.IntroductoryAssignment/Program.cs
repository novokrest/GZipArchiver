using System;
using System.Diagnostics;
using System.IO;

namespace Veeam.IntroductoryAssignment
{
    enum ProgramMode
    {
        Compress,
        Decompress
    }

    class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            var testFileName = "batman.mkv";
            args = new[] {"compress", testFileName, testFileName + ".gz"};
            //args = new[] { "decompress", testFileName + ".gz", "unpacked_" + testFileName };
#endif //DEBUG
            try
            {
                var arguments = ProgramArguments.Parse(args);
                var archiver = GZipArchiver.Instance;
                if (arguments.Mode == ProgramMode.Compress)
                {
                    archiver.Compress(arguments.OriginalFileName, arguments.ConvertedFileName);
                }
                else
                {
                    archiver.Decompress(arguments.OriginalFileName, arguments.ConvertedFileName);
                }
            }
            catch (IncorrectProgramArgumentsException e)
            {
                Console.WriteLine(e.Message +
                                  "{0}Usage: " +
                                  "{0}Compressing: {1} compress [original file name] [archive name] " +
                                  "{0}Decompressing: {1} decompress [archive name] [unpacked file name]",
                    Environment.NewLine,
                    Process.GetCurrentProcess().ProcessName);
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occurred: {0}", e.Message);
                return 1;
            }



            return 0;
        }
    }

    class ProgramArguments
    {
        private readonly ProgramMode _mode;
        private readonly string _originalFileName;
        private readonly string _convertedFileName;

        private ProgramArguments(ProgramMode mode, string originalFileName, string convertedFileName)
        {
            _mode = mode;
            _originalFileName = originalFileName;
            _convertedFileName = convertedFileName;
        }

        public ProgramMode Mode
        {
            get { return _mode; }
        }

        public string OriginalFileName
        {
            get { return _originalFileName; }
        }

        public string ConvertedFileName
        {
            get { return _convertedFileName; }
        }

        public static ProgramArguments Parse(string[] args)
        {
            if (args.Length < 3)
            {
                throw new IncorrectProgramArgumentsException("Few arguments!");
            }
            
            var originalFileName = args[1];

            var convertedFileName = args[2];
            if (File.Exists(convertedFileName))
            {
                throw new IncorrectProgramArgumentsException("Specified converted filename already exists!");
            }

            var mode = ParseMode(args[0]);

            return new ProgramArguments(mode, originalFileName, convertedFileName);
        }

        private static ProgramMode ParseMode(string arg)
        {
            if (string.Equals(arg, "compress")) return ProgramMode.Compress;
            if (string.Equals(arg, "decompress")) return ProgramMode.Decompress;
            throw new IncorrectProgramArgumentsException(String.Format("Incorrect argument: {0}", arg));
        }
    }

    class IncorrectProgramArgumentsException : Exception
    {
        public IncorrectProgramArgumentsException(string message)
            : base(message)
        {
        }
    }
}
