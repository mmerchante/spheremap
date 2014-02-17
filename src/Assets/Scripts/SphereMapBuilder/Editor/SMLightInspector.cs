using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(SMLight))]
public class SMLightInspector : Editor 
{

    private SMBuilder builder;

    public override void OnInspectorGUI()
    {
        GUILayout.BeginVertical();

        DrawBaseGUI();
        DrawTools();

        if (builder)
        {
            EditorUtility.SetDirty(builder);

            if (GUI.changed)
                builder.BuildPreviewTexture();
        }

        GUILayout.EndVertical();
        GUILayout.Space(10f);
    }

    private void DrawBaseGUI()
    {
        SMLight l = target as SMLight;      
        l.distToCenter = EditorGUILayout.Slider("Distance", l.distToCenter, 1f, 20f);
        l.theta = EditorGUILayout.Slider("Angle", l.theta, 0f, Mathf.PI * 2f);
        l.phi = EditorGUILayout.Slider("Height", l.phi, 0f, Mathf.PI);
        l.intensity = EditorGUILayout.Slider("Intensity", l.intensity, 0f, 10f);
        l.color = EditorGUILayout.ColorField("Color", l.color);

        if (GUILayout.Button("Delete"))
            GameObject.DestroyImmediate(l);

        if (!builder)
            builder = l.GetComponent<SMBuilder>();
    }

    protected virtual void DrawTools()
    {
    }
}
