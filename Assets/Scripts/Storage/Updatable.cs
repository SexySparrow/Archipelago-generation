using UnityEngine;

public class Updatable : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

#if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            UnityEditor.EditorApplication.update += NotifyUpdate;
        }
    }

    public void NotifyUpdate()
    {
        UnityEditor.EditorApplication.update -= NotifyUpdate;
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

#endif

}