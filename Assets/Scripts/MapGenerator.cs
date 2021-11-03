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
        Mesh
    }
    public DrawMode drawMode = DrawMode.NoiseMap;
    public const int chunkSize = 241;
    [Range(0, 6)]
    public int levelOfDetail;
    public float noiseScale;
    public int octave;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public float heightMultiplier;
    public AnimationCurve meshCurve;
    public bool autoUpdate;
    public TerrainType[] regions;

    Queue<MapThredInfo<MapData>> mapDataThredInfos = new Queue<MapThredInfo<MapData>> ();
    Queue<MapThredInfo<MeshData>> meshDataThredInfos = new Queue<MapThredInfo<MeshData>> ();

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.noiseMap));
                break;

            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, chunkSize, chunkSize));
                break;
            default:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, heightMultiplier, meshCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, chunkSize, chunkSize));
                break;
        }
    }

    public void RequestMapData(Action<MapData> callbak)
    {
        ThreadStart threadStart = delegate { MapDataThread(callbak); };

        new Thread(threadStart).Start();
    }

    public void MapDataThread(Action<MapData> callbak)
    {
        MapData mapData = GenerateMapData();
        lock (mapDataThredInfos)
        {
            mapDataThredInfos.Enqueue(new MapThredInfo<MapData>(callbak, mapData));
        }
    }
    
    public void RequestMeshData(MapData mapData, Action<MeshData> callbak)
    {
        ThreadStart threadStart = delegate { MeshDataThread(mapData, callbak); };

        new Thread(threadStart).Start();
    }

    public void MeshDataThread(MapData mapData, Action<MeshData> callbak)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh (mapData.noiseMap, heightMultiplier, meshCurve, levelOfDetail);
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
                thredInfo.callback (thredInfo.param);
            }
        }

        if (meshDataThredInfos.Count > 0)
        {
            for (int i = 0; i < meshDataThredInfos.Count; i++)
            {
                MapThredInfo<MeshData> thredInfo = meshDataThredInfos.Dequeue();
                thredInfo.callback (thredInfo.param);
            }
        }
    }

    MapData GenerateMapData()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(chunkSize, chunkSize, seed, noiseScale, octave, persistance, lacunarity, offset);

        Color[] colorMap = new Color[chunkSize * chunkSize];
        for (int y_val = 0; y_val < chunkSize; y_val++)
        {
            for (int x_val = 0; x_val < chunkSize; x_val++)
            {
                float currentH = noiseMap[x_val, y_val];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentH <= regions[i].height)
                    {
                        colorMap[y_val * chunkSize + x_val] = regions[i].color;
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
