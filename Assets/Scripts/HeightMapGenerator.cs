using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{
    public static NoiseMap GenerateHeightMap(int width, int height, NoiseStorage settings, Vector2 sampleCentre)
    {
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.settings, sampleCentre);

        AnimationCurve heightCurve = new AnimationCurve(settings.meshCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                values[x, y] *= heightCurve.Evaluate(values[x, y]) * settings.heightMultiplier;

                if (values[x, y] > maxValue)
                {
                    maxValue = values[x, y];
                }
                if (values[x, y] < minValue)
                {
                    minValue = values[x, y];
                }
            }
        }

        return new NoiseMap(values, minValue, maxValue);
    }
}

public struct NoiseMap
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public NoiseMap(float[,] noiseMap , float minValue, float maxValue)
    {
        this.values = noiseMap;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}