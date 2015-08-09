using System.IO;
using Veeam.IntroductoryAssignment.FileSplitting;

namespace Veeam.IntroductoryAssignment.FileAssembling
{
    class ArchiveFileAssembler : FileAssembler
    {
        public ArchiveFileAssembler(string fileName, long chunkCount)
            : base(fileName, chunkCount)
        {
        }

        protected override void CompleteAssembling()
        {
            using (var fileStream = new FileStream(FileName, FileMode.Append, FileAccess.Write))
            {
                var headerWriter = new ArchiveHeaderWriter(fileStream);
                headerWriter.WriteHeader(ArchiveHeader.Create(SplitInfo));
            }
            TaskPool.Stop();
        }
    }
}
