using System;
using System.Diagnostics;

namespace Veeam.TestTask
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
            try
            {
                ProgramArguments arguments = ProgramArguments.Parse(args);
                IArchiver archiver = GZipArchiver.Instance;
                if (arguments.Mode == ProgramMode.Compress)
                {
                    archiver.Compress(arguments.FilePath, arguments.ArchivePath);
                }
                else
                {
                    archiver.Decompress(arguments.ArchivePath, arguments.FilePath);
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
            catch (Exception)
            {
                return 1;
            }



            return 0;
        }
    }

    class ProgramArguments
    {
        private readonly ProgramMode _mode;
        private readonly string _filePath;
        private readonly string _archivePath;

        private ProgramArguments(ProgramMode mode, string filePath, string archivePath)
        {
            _mode = mode;
            _filePath = filePath;
            _archivePath = archivePath;
        }

        public ProgramMode Mode
        {
            get { return _mode; }
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        public string ArchivePath
        {
            get { return _archivePath; }
        }

        public static ProgramArguments Parse(string[] args)
        {
            if (args.Length < 3) throw new IncorrectProgramArgumentsException("Few arguments!");
            var mode = ParseMode(args[0]);
            return mode == ProgramMode.Compress ? new ProgramArguments(mode, args[1], args[2]) : new ProgramArguments(mode, args[2], args[1]);
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
