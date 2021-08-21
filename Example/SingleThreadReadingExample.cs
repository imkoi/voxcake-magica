namespace VoxCake.Magica
{
    internal class SingleThreadReadingExample
    {
        private const int FileCount = 5;
        private const string FullPathToVxlFile = "C:/your/path/to/vxl/file{0}.vxl";
        
        public void Main()
        {
            var magicaReader = new MagicaReader();
            var magicaFiles = new MagicaFile[FileCount];
            
            for (var i = 0; i < FileCount; i++)
            {
                var magicaFile = magicaReader.LoadFile(GetFilePath(i));
                
                magicaFiles[i] = magicaFile;
            }

            foreach (var magicaFile in magicaFiles)
            {
                var chunks = magicaFile.Chunks;
                var palette = magicaFile.Palette;
            }
        }

        private string GetFilePath(int fileIndex)
        {
            return string.Format(FullPathToVxlFile, fileIndex);
        }
    }
}