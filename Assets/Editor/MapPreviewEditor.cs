using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapPreview mapGen = (MapPreview)target;

        if (DrawDefaultInspector() && mapGen.autoUpdate)
        {
            mapGen.DrawMapInEditor();
        }

        if (GUILayout.Button("Generate Map"))
        {
            mapGen.DrawMapInEditor();
        }
    }
}
