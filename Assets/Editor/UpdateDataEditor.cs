using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Updateable), true)]
public class UpdateDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Updateable data = (Updateable)target;

        if (GUILayout.Button("Update"))
        {
            data.NotifyUpdate();
            EditorUtility.SetDirty(target);
        }
    }
}
