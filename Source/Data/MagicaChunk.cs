using UnityEngine;

namespace VoxCake.Magica
{
    public struct MagicaChunk
    {
        public Vector3Int size;
        public Vector3Int position;
        public Vector3Int rotation;
        public MagicaVoxel[] voxels;
    }
}