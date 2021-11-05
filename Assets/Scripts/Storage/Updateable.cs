using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Updateable : ScriptableObject
{

    public event System.Action OnUpdate;
    public bool update;

    protected virtual void OnValidate() {
        if (update) {
            NotifyUpdate();
        }
    }
    public void NotifyUpdate()
    {
        if (OnUpdate != null)
        {
            OnUpdate();
        }
    }

}
