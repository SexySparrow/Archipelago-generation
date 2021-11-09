using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainStorage : Updateable
{
    public float scale = 4f;
    public bool fallOffMap;
    public float heightMultiplier;
    public AnimationCurve meshCurve;

    public float minHeight
    {
        get
        {
            return scale * heightMultiplier * meshCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return scale * heightMultiplier * meshCurve.Evaluate(1);
        }
    }
}
