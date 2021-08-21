using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoxCake.Magica
{
    internal class MultithreadingExample
    {
        private const int FileCount = 5;
        private const string FullPathToVxlFile = "C:/your/path/to/vxl/file{0}.vxl";
        
        public void Main()
        {
            var magicaFiles = new List<MagicaFile>();
            var tasks = new Task[FileCount];
            
            for (var i = 0; i < FileCount; i++)
            {
                var fileIndex = i;
                
                var task = Task.Factory.StartNew(() =>
                {
                    LoadFile(GetFilePath(fileIndex), magicaFiles);
                });

                tasks[i] = task;
            }

            Task.WaitAll(tasks);

            foreach (var magicaFile in magicaFiles)
            {
                var chunks = magicaFile.Chunks;
                var palette = magicaFile.Palette;
            }
        }

        private void LoadFile(string filePath, List<MagicaFile> magicaFiles)
        {
            var magicaReader = new MagicaReader();
            var magicaFile = magicaReader.LoadFile(filePath);
            
            magicaFiles.Add(magicaFile);
        }

        private string GetFilePath(int fileIndex)
        {
            return string.Format(FullPathToVxlFile, fileIndex);
        }
    }
}