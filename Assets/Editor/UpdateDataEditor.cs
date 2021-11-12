using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Updatable), true)]
public class UpdateDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Updatable data = (Updatable)target;

        if (GUILayout.Button("Update"))
        {
            data.NotifyUpdate();
            EditorUtility.SetDirty(target);
        }
    }
}
