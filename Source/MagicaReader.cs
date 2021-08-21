using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace VoxCake.Magica
{
    public class MagicaReader : IMagicaReader
    {
        private readonly byte[] _buffer;
        private readonly List<MagicaModel> _models;
        private readonly List<MagicaChunk> _chunks;

        public MagicaReader()
        {
            _buffer = new byte[4];
            _models = new List<MagicaModel>();
            _chunks = new List<MagicaChunk>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MagicaFile LoadFile(string filePath)
        {
            var fileStream = File.OpenRead(filePath);
            var binaryStream = new BinaryReader(fileStream);
            var palette = new Color32[MagicaConstants.PaletteSize];
            var stream = binaryStream.BaseStream;

            var currentSize = new Vector3Int();
            var currentChunkTransform = new MagicaTransform();
            
            var voxVersion = binaryStream.ReadMagicaVersion();
            
            if (voxVersion >= 150)
            {
                while (stream.Position < stream.Length)
                {
                    var segment = binaryStream.ReadMagicaSegment(_buffer);
                    var isDone = false;

                    switch (segment.name)
                    {
                        case MagicaConstants.ChunkSizeSegment:
                            currentSize = binaryStream.ReadMagicaVector();
                            break;
                        case MagicaConstants.ChunkVoxelsSegment:
                            var voxels = binaryStream.ReadMagicaVoxels(_buffer);
                            var model = new MagicaModel
                            {
                                size = currentSize,
                                voxels = voxels
                            };
                            _models.Add(model);
                            break;
                        case MagicaConstants.ChunkTransformSegment:
                            currentChunkTransform = binaryStream.ReadMagicaChunkTransform();
                            break;
                        case MagicaConstants.ChunkShapeSegment:
                            var modelIndex = binaryStream.ReadMagicaModelIndex();
                            var chunkModel = _models[modelIndex];
                            var chunk = new MagicaChunk
                            {
                                size = chunkModel.size,
                                position = currentChunkTransform.position,
                                rotation = currentChunkTransform.rotation,
                                voxels = chunkModel.voxels
                            };
                            _chunks.Add(chunk);
                            break;
                        case MagicaConstants.ChunkPaletteSegment:
                            binaryStream.ReadMagicaPalette(palette, _buffer);
                            isDone = true;
                            break;
                        default:
                            stream.Position += segment.size;
                            break;
                    }

                    if (isDone)
                    {
                        break;
                    }
                }
            }

            var chunks = _chunks.ToArray();

            _models.Clear();
            _chunks.Clear();
            fileStream.Dispose();
            binaryStream.Dispose();

            return new MagicaFile(palette, chunks);
        }
    }
}