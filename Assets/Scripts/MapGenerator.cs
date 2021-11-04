using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh,
        FalloffMap
    }

    public Noise.TerrainMode terrainMode;
    public DrawMode drawMode = DrawMode.NoiseMap;
    public const int chunkSize = 239;
    [Range(0, 6)]
    public int editorLOD;
    public float noiseScale;
    public int octave;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public bool fallOffMap;
    public float heightMultiplier;
    public AnimationCurve meshCurve;
    public bool autoUpdate;
    public TerrainType[] regions;
    float[,] falloffMap;

    Queue<MapThredInfo<MapData>> mapDataThredInfos = new Queue<MapThredInfo<MapData>>();
    Queue<MapThredInfo<MeshData>> meshDataThredInfos = new Queue<MapThredInfo<MeshData>>();

    void Awake()
    {
        falloffMap = FalloffMapGenerator.GenerateFalloffMap(chunkSize);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(new Vector2(0, 0));
        MapDisplay display = FindObjectOfType<MapDisplay>();
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.noiseMap));
                break;

            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, chunkSize, chunkSize));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, heightMultiplier, meshCurve, editorLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, chunkSize, chunkSize));
                break;
            case DrawMode.FalloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffMapGenerator.GenerateFalloffMap(chunkSize)));
                break;
            default:
                break;
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callbak)
    {
        ThreadStart threadStart = delegate { MapDataThread(center, callbak); };

        new Thread(threadStart).Start();
    }

    public void MapDataThread(Vector2 center, Action<MapData> callbak)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThredInfos)
        {
            mapDataThredInfos.Enqueue(new MapThredInfo<MapData>(callbak, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callbak)
    {
        ThreadStart threadStart = delegate { MeshDataThread(mapData, lod, callbak); };

        new Thread(threadStart).Start();
    }

    public void MeshDataThread(MapData mapData, int lod, Action<MeshData> callbak)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, heightMultiplier, meshCurve, lod);
        lock (meshDataThredInfos)
        {
            meshDataThredInfos.Enqueue(new MapThredInfo<MeshData>(callbak, meshData));
        }
    }

    void Update()
    {
        if (mapDataThredInfos.Count > 0)
        {
            for (int i = 0; i < mapDataThredInfos.Count; i++)
            {
                MapThredInfo<MapData> thredInfo = mapDataThredInfos.Dequeue();
                thredInfo.callback(thredInfo.param);
            }
        }

        if (meshDataThredInfos.Count > 0)
        {
            for (int i = 0; i < meshDataThredInfos.Count; i++)
            {
                MapThredInfo<MeshData> thredInfo = meshDataThredInfos.Dequeue();
                thredInfo.callback(thredInfo.param);
            }
        }
    }

    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(chunkSize + 2, chunkSize + 2, seed, noiseScale, octave, persistance, lacunarity, center + offset, terrainMode);

        Color[] colorMap = new Color[chunkSize * chunkSize];
        for (int y_val = 0; y_val < chunkSize; y_val++)
        {
            for (int x_val = 0; x_val < chunkSize; x_val++)
            {
                if (fallOffMap)
                {
                    noiseMap[x_val, y_val] = Mathf.Clamp01(noiseMap[x_val, y_val] - falloffMap[x_val, y_val]);
                }

                float currentH = noiseMap[x_val, y_val];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentH >= regions[i].height)
                    {
                        colorMap[y_val * chunkSize + x_val] = regions[i].color;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    private void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        if (octave < 0)
        {
            octave = 0;
        }

        falloffMap = FalloffMapGenerator.GenerateFalloffMap(chunkSize);
    }

    struct MapThredInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T param;

        public MapThredInfo(Action<T> callback, T param)
        {
            this.callback = callback;
            this.param = param;
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

public struct MapData
{
    public readonly float[,] noiseMap;
    public readonly Color[] colorMap;

    public MapData(float[,] noiseMap, Color[] colorMap)
    {
        this.noiseMap = noiseMap;
        this.colorMap = colorMap;
    }
}
