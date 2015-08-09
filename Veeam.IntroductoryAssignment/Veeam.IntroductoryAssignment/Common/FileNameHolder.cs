namespace Veeam.IntroductoryAssignment.Common
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
