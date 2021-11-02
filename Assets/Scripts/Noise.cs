using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapW, int mapH, int seed, float scale, int octave, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapW, mapH];
        System.Random rng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octave];

        for (int i = 0; i < octave; i++)
        {
            float offsetX = rng.Next(-100000, 100000) + offset.x;
            float offsetY = rng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale == 0)
        {
            scale = 0.001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        float halfW = mapW / 2f;
        float halfH = mapH / 2f;

        for (int y_val = 0; y_val < mapH; y_val++)
        {
            for (int x_val = 0; x_val < mapW; x_val++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octave; i++)
                {
                    float y_float_val = (y_val - halfH) / scale * frequency + octaveOffsets[i].y;
                    float x_float_val = (x_val - halfW) / scale * frequency + octaveOffsets[i].x;

                    float perlinValue = Mathf.PerlinNoise(x_float_val, y_float_val) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }

                if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x_val, y_val] = noiseHeight;
            }
        }

        for (int y_val = 0; y_val < mapH; y_val++)
        {
            for (int x_val = 0; x_val < mapW; x_val++)
            {
                noiseMap[x_val, y_val] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x_val, y_val]);
            }
        }

        return noiseMap;
    }
}
