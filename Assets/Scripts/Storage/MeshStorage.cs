using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshStorage : Updateable
{
    public float scale = 4f;

    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    [Range(0, numSupportedChunkSizes - 1)]
    public int mapChunkSizeIndex;

    public int vertsPerLine
    {
        get
        {
            return supportedChunkSizes[mapChunkSizeIndex] + 1;
        }
    }

    public float worldSize {

        get {
            return (vertsPerLine - 3) * scale;
        }
    }
}
