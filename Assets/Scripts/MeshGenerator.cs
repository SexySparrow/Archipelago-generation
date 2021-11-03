using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMulti, AnimationCurve meshCurve, int detail)
    {
        AnimationCurve animationCurve = new AnimationCurve(meshCurve.keys);
        
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int inc = (detail == 0) ? 1 : detail * 2;
        int verticesLineCount = (width - 1) / inc + 1;

        MeshData meshData = new MeshData(verticesLineCount, verticesLineCount);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += inc)
        {
            for (int x = 0; x < width; x += inc)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, animationCurve.Evaluate(heightMap[x, y]) * heightMulti, topLeftZ - y);
                meshData.UV[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesLineCount + 1, vertexIndex + verticesLineCount);
                    meshData.AddTriangle(vertexIndex + verticesLineCount + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] UV;
    int triangleIndex;

    public MeshData(int meshW, int meshH)
    {
        vertices = new Vector3[meshW * meshH];
        UV = new Vector2[meshW * meshH];
        triangles = new int[(meshW - 1) * (meshH - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;

        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = UV;
        mesh.RecalculateNormals();

        return mesh;
    }

}
