using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veeam.IntroductoryAssignment.FileDataManaging
{
    class FileNameHolder
    {
        private readonly string _fileName;

        public FileNameHolder(string fileName)
        {
            _fileName = fileName;
        }

        public string FileName
        {
            get { return _fileName; }
        }
    }
}
