namespace Veeam.IntroductoryAssignment.FileAssembling
{
    class RegularFileAssembler : FileAssembler
    {
        public RegularFileAssembler(string fileName, long chunkCount)
            : base(fileName, chunkCount)
        {
        }

        protected override void CompleteAssembling()
        {
        }
    }
}
