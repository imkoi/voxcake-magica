using UnityEngine;

namespace VoxCake.Magica
{
    public class MagicaFile
    {
        public Color32[] Palette { get; }
        public MagicaChunk[] Chunks { get; }

        public MagicaFile(Color32[] palette, MagicaChunk[] chunks)
        {
            Palette = palette;
            Chunks = chunks;
        }
    }
}