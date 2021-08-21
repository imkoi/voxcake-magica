using UnityEngine;

namespace VoxCake.Magica
{
    internal class ParsingExample
    {
        public MapModel ParseMagicaToMapModel(MagicaFile magicaFile)
        {
            var magicaChunks = magicaFile.Chunks;

            var minPosition = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
            var maxPosition = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
            var magicaSize = new Vector3();

            for (var chunkIndex = 0; chunkIndex < magicaChunks.Length; chunkIndex++)
            {
                var magicaChunk = magicaChunks[chunkIndex];
                var defaultPosition = magicaChunk.position;
                var chunkSize = magicaChunk.size;
                var halfChunkSize = new Vector3Int(
                    chunkSize.x / 2,
                    chunkSize.y / 2,
                    chunkSize.z / 2
                );
                var chunkPosition = defaultPosition - halfChunkSize;
                
                magicaChunk.position = chunkPosition;

                for (var i = 0; i < 3; i++)
                {
                    if (chunkPosition[i] < minPosition[i])
                    {
                        minPosition[i] = chunkPosition[i];
                    }
                }
                
                for (var i = 0; i < 3; i++)
                {
                    if (chunkPosition[i] + chunkSize[i] > maxPosition[i])
                    {
                        maxPosition[i] = chunkPosition[i] + chunkSize[i];
                    }
                }

                for (var i = 0; i < 3; i++)
                {
                    magicaSize[i] = maxPosition[i] - minPosition[i];
                }
                
                magicaChunks[chunkIndex] = magicaChunk;
            }

            var volumeSize = new Vector3Int(
                Mathf.CeilToInt(magicaSize.x),
                Mathf.CeilToInt(magicaSize.y),
                Mathf.CeilToInt(magicaSize.z));
            
            var mapModel = new MapModel(volumeSize, magicaFile.Palette);

            foreach (var chunk in magicaChunks)
            {
                var voxels = chunk.voxels;
                var chunkPosition = new Vector3Int(chunk.position.x, chunk.position.y, chunk.position.z);
                var positionAdjustment = chunkPosition - minPosition;
                var rotation = chunk.rotation;
                var size = chunk.size;

                foreach (var magicaVoxel in voxels)
                {
                    var voxelPosition = new Vector3Int(magicaVoxel.x, magicaVoxel.y, magicaVoxel.z);
                    
                    for (var i = 0; i < 3; i++)
                    {
                        var multiplier = rotation[i];
                        
                        if (multiplier != 1)
                        {
                            var value = voxelPosition[i] * multiplier;
                            voxelPosition[i] = size[i] + value - 1;
                        }
                    }

                    var position = voxelPosition + positionAdjustment;

                    mapModel.SetVoxel(position, magicaVoxel.colorIndex);
                }
            }

            return mapModel;
        }
    }
    
    internal class MapModel
    {
        private readonly Vector3Int _mapSize;
        private readonly Color32[] _mapPalette;
        private readonly byte[] _mapBuffer;

        public MapModel(Vector3Int mapSize, Color32[] mapPalette)
        {
            _mapSize = mapSize;
            _mapPalette = mapPalette;

            _mapBuffer = new byte[mapSize.x * mapSize.y * mapSize.z];
        }

        public void SetVoxel(Vector3Int position, byte voxel)
        {
            var index = (position.x * _mapSize.y + position.y) * _mapSize.z + position.z;

            _mapBuffer[index] = voxel;
        }
    }
}