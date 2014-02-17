using UnityEngine;
using System.Collections;

public class SMViewer : MonoBehaviour 
{
    public Texture[] sphereMaps;

    public Material material;

    private int index = 0;


    private float fps = 0f;
    private float fpsVel = 0f;

    public void OnGUI()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Previous", GUILayout.MinHeight(100f), GUILayout.MinWidth(100f)))
        {
            index = (index - 1 < 0) ? sphereMaps.Length - 1 : index - 1;
            UpdateMaterial();
        }

        if (GUILayout.Button("Next", GUILayout.MinHeight(100f), GUILayout.MinWidth(100f)))
        {
            index = (index + 1 >= sphereMaps.Length) ? 0 : index + 1;
            UpdateMaterial();
        }

        this.fps = Mathf.SmoothDamp(fps, 1f / Time.deltaTime, ref fpsVel, .25f);

        GUI.color = Color.red;
        GUILayout.Label(fps.ToString("0"));

        GUILayout.EndHorizontal();
    }

    private void UpdateMaterial()
    {
        if(index < sphereMaps.Length)
            this.material.SetTexture("_MainTex", sphereMaps[index]);
    }
}
 