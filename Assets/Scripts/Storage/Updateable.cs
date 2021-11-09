using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Updateable : ScriptableObject
{

    public event System.Action OnUpdate;
    public bool update;

    protected virtual void OnValidate() {
        if (update) {
            UnityEditor.EditorApplication.update += NotifyUpdate;
        }
    }
    public void NotifyUpdate()
    {
        UnityEditor.EditorApplication.update -= NotifyUpdate;
        if (OnUpdate != null)
        {
            OnUpdate();
        }
    }

}
