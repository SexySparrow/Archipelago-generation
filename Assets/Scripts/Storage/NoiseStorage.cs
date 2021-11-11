using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseStorage : Updateable
{
    public NoiseSettings settings;
    
    public bool fallOffMap;
    public float heightMultiplier;
    public AnimationCurve meshCurve;

    public float minHeight
    {
        get
        {
            return heightMultiplier * meshCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return heightMultiplier * meshCurve.Evaluate(1);
        }
    }

    #if UNITY_EDITOR
    protected override void OnValidate() {
        settings.ValidateVal();
        
        base.OnValidate();
    }
    #endif
}
