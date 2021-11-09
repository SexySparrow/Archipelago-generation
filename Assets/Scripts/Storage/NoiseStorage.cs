using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseStorage : Updateable
{
    public float noiseScale;
    public int octave;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public Noise.TerrainMode terrainMode;

    #if UNITY_EDITOR
    protected override void OnValidate() {
        if (noiseScale <= 0) {
            noiseScale = 0.0001f;
        }
        if (lacunarity <= 1) {
            lacunarity = 1.0001f;
        }
        
        base.OnValidate();
    }
    #endif
}
