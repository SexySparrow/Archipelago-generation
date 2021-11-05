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
}
