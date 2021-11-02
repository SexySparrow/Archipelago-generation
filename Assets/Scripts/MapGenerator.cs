using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh
    }
    public DrawMode drawMode = DrawMode.NoiseMap;
    public int mapW;
    public int mapH;
    public float noiseScale;
    public int octave;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public bool autoUpdate;
    public TerrainType[] regions;
    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapW, mapH, seed, noiseScale, octave, persistance, lacunarity, offset);

        Color[] colorMap = new Color[mapW * mapH];
        for (int y_val = 0; y_val < mapH; y_val++)
        {
            for (int x_val = 0; x_val < mapW; x_val++)
            {
                float currentH = noiseMap[x_val, y_val];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentH <= regions[i].height)
                    {
                        colorMap[y_val * mapW + x_val] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapW, mapH));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh (MeshGenerator.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColorMap (colorMap, mapW, mapH));
        }
    }

    private void OnValidate()
    {
        if (mapH < 1)
        {
            mapH = 1;
        }

        if (mapW < 1)
        {
            mapW = 1;
        }

        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        if (octave < 0)
        {
            octave = 0;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
