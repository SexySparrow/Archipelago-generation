using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public TerrainStorage terrainStorage;
    public NoiseStorage noiseStorage;
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh,
        FalloffMap
    }
    public DrawMode drawMode = DrawMode.NoiseMap;
    public const int chunkSize = 239;
    [Range(0, 6)]
    public int editorLOD;
    public bool autoUpdate;
    public TerrainType[] regions;
    float[,] falloffMap;

    Queue<MapThredInfo<MapData>> mapDataThredInfos = new Queue<MapThredInfo<MapData>>();
    Queue<MapThredInfo<MeshData>> meshDataThredInfos = new Queue<MapThredInfo<MeshData>>();

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
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
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, terrainStorage.heightMultiplier, terrainStorage.meshCurve, editorLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, chunkSize, chunkSize));
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
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.noiseMap, terrainStorage.heightMultiplier, terrainStorage.meshCurve, lod);
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
        float[,] noiseMap = Noise.GenerateNoiseMap(chunkSize + 2, chunkSize + 2, noiseStorage.seed, noiseStorage.noiseScale, noiseStorage.octave, noiseStorage.persistance, noiseStorage.lacunarity, center + noiseStorage.offset, noiseStorage.terrainMode);

        if (terrainStorage.fallOffMap)
        {
            if (falloffMap == null)
            {
                falloffMap = FalloffMapGenerator.GenerateFalloffMap(chunkSize + 2);
            }
        }

        Color[] colorMap = new Color[chunkSize * chunkSize];
        for (int y_val = 0; y_val < chunkSize + 2; y_val++)
        {
            for (int x_val = 0; x_val < chunkSize + 2; x_val++)
            {
                if (terrainStorage.fallOffMap)
                {
                    noiseMap[x_val, y_val] = Mathf.Clamp01(noiseMap[x_val, y_val] - falloffMap[x_val, y_val]);
                }

                if (x_val < chunkSize && y_val < chunkSize)
                {
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
        }

        return new MapData(noiseMap, colorMap);
    }

    private void OnValidate()
    {
        if (terrainStorage != null)
        {
            terrainStorage.OnUpdate -= OnValuesUpdated;
            terrainStorage.OnUpdate += OnValuesUpdated;
        }
        if (noiseStorage != null)
        {
            noiseStorage.OnUpdate -= OnValuesUpdated;
            noiseStorage.OnUpdate += OnValuesUpdated;
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
