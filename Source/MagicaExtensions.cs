using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace VoxCake.Magica
{
    internal static class MagicaExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ReadMagicaVersion(this BinaryReader binaryReader)
        {
            binaryReader.Skip(4);
            return binaryReader.ReadInt32();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MagicaSegment ReadMagicaSegment(this BinaryReader binaryReader, byte[] buffer)
        {
            binaryReader.Read(buffer, 0, 4);
            
            var name = Encoding.UTF8.GetString(buffer, 0, 4);
            var size = binaryReader.ReadInt32();
            binaryReader.Skip(4);

            return new MagicaSegment
            {
                name = name,
                size = size
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector3Int ReadMagicaVector(this BinaryReader binaryReader)
        {
            var x = binaryReader.ReadInt32();
            var z = binaryReader.ReadInt32();
            var y = binaryReader.ReadInt32();

            return new Vector3Int(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MagicaVoxel[] ReadMagicaVoxels(this BinaryReader binaryReader, byte[] buffer)
        {
            var voxelsCount = binaryReader.ReadInt32();
            var voxelData = new MagicaVoxel[voxelsCount];

            for (var i = 0; i < voxelsCount; i++)
            {
                binaryReader.Read(buffer, 0, 4);
                voxelData[i] = new MagicaVoxel
                {
                    x = buffer[0],
                    y = buffer[2],
                    z = buffer[1],
                    colorIndex = buffer[3]
                };
            }

            return voxelData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MagicaTransform ReadMagicaChunkTransform(this BinaryReader binaryReader)
        {
            binaryReader.Skip(4);
            binaryReader.SkipMagicaDictionary();
            binaryReader.Skip(16);

            return binaryReader.ReadMagicaTransform();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ReadMagicaModelIndex(this BinaryReader binaryReader)
        {
            binaryReader.Skip(4);
            binaryReader.SkipMagicaDictionary();
            binaryReader.Skip(4);
            
            var modelId = binaryReader.ReadInt32();
            binaryReader.SkipMagicaDictionary();
            BitConverter.ToString(new byte[32], 0, 23);

            return modelId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Color32[] ReadMagicaPalette(this BinaryReader binaryReader,
            Color32[] palette, byte[] buffer)
        {
            for (var i = 0; i < MagicaConstants.PaletteSize; i++)
            {
                binaryReader.Read(buffer, 0, 4);

                palette[i] = new Color32
                {
                    r = buffer[0],
                    g = buffer[1],
                    b = buffer[2],
                    a = buffer[3],
                };
            }

            return palette;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MagicaTransform ReadMagicaTransform(this BinaryReader binaryReader)
        {
            var count = binaryReader.ReadInt32();
            var rotation = Vector3Int.one;
            var position = new Vector3Int();
            
            if (count == 2)
            {
                binaryReader.Skip(6); // _r
                rotation = binaryReader.ReadMagicaString().ToMatrix3();
                
                binaryReader.Skip(6); // _t
                position = binaryReader.ReadMagicaString().ToVector3Int();
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    binaryReader.Skip(6); // _t
                    position = binaryReader.ReadMagicaString().ToVector3Int();
                }
            }

            return new MagicaTransform
            {
                position = position,
                rotation = rotation
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Dictionary<string, string> ReadMagicaDictionary(this BinaryReader binaryReader)
        {
            var pairsCount = binaryReader.ReadInt32();
            var dictionary = new Dictionary<string, string>(pairsCount);

            for (var i = 0; i < pairsCount; i++)
            {
                var key = binaryReader.ReadMagicaString();
                var value = binaryReader.ReadMagicaString();

                dictionary.Add(key, value);
            }

            return dictionary;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string ReadMagicaString(this BinaryReader binaryReader)
        {
            var length = binaryReader.ReadInt32();
            var buffer = binaryReader.ReadChars(length);

            return new string(buffer);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Skip(this BinaryReader binaryReader, int count)
        {
            binaryReader.BaseStream.Position += count;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SkipMagicaString(this BinaryReader binaryReader)
        {
            var stringLength = binaryReader.ReadInt32();
            binaryReader.Skip(stringLength);

            return stringLength;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SkipMagicaDictionary(this BinaryReader binaryReader)
        {
            var pairsCount = binaryReader.ReadInt32();
            var skipCount = 4;
            
            for (var i = 0; i < pairsCount; i++)
            {
                skipCount += binaryReader.SkipMagicaString();
                skipCount += binaryReader.SkipMagicaString();
            }

            return skipCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector3Int ToVector3Int(this string stringVector)
        {
            var strings = stringVector.Split(' ');

            return new Vector3Int
            {
                x = int.Parse(strings[0]),
                y = int.Parse(strings[2]),
                z = int.Parse(strings[1]),
            };
        }

        internal static Vector3Int ToMatrix3(this string stringMatrix)
        {
            var rotationValue = byte.Parse(stringMatrix);
            var matrix = Vector3Int.zero;

            var firstRowValue = (rotationValue & 8) == 0
                ? 1
                : -1;
            var secondRowValue = (rotationValue & 16) == 0
                ? 1
                : -1;
            var thirdRowValue = (rotationValue & 32) == 0
                ? 1
                : -1;

            matrix.x = firstRowValue;
            matrix.y = -secondRowValue;
            matrix.z = thirdRowValue;

            return matrix;
        }
    }
}