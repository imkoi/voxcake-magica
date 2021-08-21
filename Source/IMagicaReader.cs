namespace VoxCake.Magica
{
    public interface IMagicaReader
    {
        MagicaFile LoadFile(string filePath);
    }
}