using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public MeshStorage meshStorage;
    public NoiseStorage noiseStorage;
    public TextureData textureData;
    public Material terrainMaterial;
    public enum DrawMode
    {
        NoiseMap,
        Mesh,
        FalloffMap
    }
    public DrawMode drawMode = DrawMode.NoiseMap;
    [Range(0, MeshStorage.numSupportedLODs-1)]
    public int editorLOD;
    public bool autoUpdate;
    float[,] falloffMap;

    Queue<MapThredInfo<NoiseMap>> mapDataThredInfos = new Queue<MapThredInfo<NoiseMap>>();
    Queue<MapThredInfo<MeshData>> meshDataThredInfos = new Queue<MapThredInfo<MeshData>>();

    private void Start()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateHeights(terrainMaterial, noiseStorage.minHeight, noiseStorage.maxHeight);
    }
    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public void DrawMapInEditor()
    {
        if (noiseStorage == null)
        {
            Debug.LogError("Noise Storage is null");
            return;
        }
        textureData.UpdateHeights(terrainMaterial, noiseStorage.minHeight, noiseStorage.maxHeight);
        NoiseMap mapData = HeightMapGenerator.GenerateHeightMap(meshStorage.vertsPerLine, meshStorage.vertsPerLine, noiseStorage, Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.values));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.values, meshStorage, editorLOD));
                break;
            case DrawMode.FalloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffMapGenerator.GenerateFalloffMap(meshStorage.vertsPerLine)));
                break;
            default:
                break;
        }
    }

    public void RequestMapData(Vector2 center, Action<NoiseMap> callbak)
    {
        ThreadStart threadStart = delegate { MapDataThread(center, callbak); };

        new Thread(threadStart).Start();
    }

    public void MapDataThread(Vector2 center, Action<NoiseMap> callbak)
    {
        NoiseMap mapData = HeightMapGenerator.GenerateHeightMap(meshStorage.vertsPerLine, meshStorage.vertsPerLine, noiseStorage, center);
        lock (mapDataThredInfos)
        {
            mapDataThredInfos.Enqueue(new MapThredInfo<NoiseMap>(callbak, mapData));
        }
    }

    public void RequestMeshData(NoiseMap mapData, int lod, Action<MeshData> callbak)
    {
        ThreadStart threadStart = delegate { MeshDataThread(mapData, lod, callbak); };

        new Thread(threadStart).Start();
    }

    public void MeshDataThread(NoiseMap mapData, int lod, Action<MeshData> callbak)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.values, meshStorage, lod);
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
                MapThredInfo<NoiseMap> thredInfo = mapDataThredInfos.Dequeue();
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

    private void OnValidate()
    {
        if (meshStorage != null)
        {
            meshStorage.OnUpdate -= OnValuesUpdated;
            meshStorage.OnUpdate += OnValuesUpdated;
        }
        if (noiseStorage != null)
        {
            noiseStorage.OnUpdate -= OnValuesUpdated;
            noiseStorage.OnUpdate += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnUpdate -= OnTextureValuesUpdated;
            textureData.OnUpdate += OnTextureValuesUpdated;
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
