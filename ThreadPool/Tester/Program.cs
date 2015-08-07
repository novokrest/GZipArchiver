using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            string solutionPath = 
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(Environment.CurrentDirectory)));

            string programPath = solutionPath + @"\ThreadPool\bin\Debug\ThreadPool.exe";
            string testFilePath = Path.GetDirectoryName(programPath) + @"\test.txt";
            Console.WriteLine("{0}\n\n{1}", programPath, testFilePath);

            var tester = new ArchiveTester(programPath, testFilePath);
            tester.Test();

            new object();
        }
    }
}
