using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor (typeof(SMBuilder))]
public class SMInspector : Editor 
{
    private enum LightType
    {
        Point,
        Directional,
        Sphere
    }

    private LightType lightType = LightType.Directional;

    public void OnEnable()
    {
        SMBuilder maker = target as SMBuilder;
        maker.EnablePreview();
    }

    public void OnDisable()
    {
        SMBuilder maker = target as SMBuilder;
        maker.DisablePreview();
    }

    private bool diffuseFoldout = true;
    private bool specularFoldout = false;
    private bool emissiveFoldout = false;

    private bool showTools = true;

    public override void OnInspectorGUI()
    {
        SMBuilder maker = target as SMBuilder;

        EditorGUIUtility.labelWidth = 100f;
        GUI.changed = false;       

        GUILayout.Space(5f);

        GUILayout.BeginVertical("Matcap Maker", "Window", GUILayout.MinHeight(200f));

        GUILayout.Label(maker.sphereMap, "Button", GUILayout.MinHeight(200f), GUILayout.ExpandWidth(true));

        /*
        Rect r = EditorGUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        EditorGUI.ProgressBar(r, 1f, "Rendering...");
        GUILayout.Space(20f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        */

        {
            GUILayout.Space(5f);
       
            maker.aaQuality = (int)EditorGUILayout.IntSlider ("Samples", maker.aaQuality, 1, 5);
            showTools = EditorGUILayout.Toggle("Show properties", showTools);

        }

        if (showTools)
        {
            // Surface
            {
                GUILayout.Space(10f);
                GUILayout.BeginVertical("Surface", "Window");
                EditorGUI.indentLevel++;

                // Diffuse
                {
                    if (GUILayout.Button("Diffuse", "Foldout"))
                        diffuseFoldout = !diffuseFoldout;

                    if (diffuseFoldout)
                    {
                        EditorGUI.indentLevel++;
                        maker.diffuseColor = EditorGUILayout.ColorField("Diffuse", maker.diffuseColor);
                        maker.diffuseWeight = EditorGUILayout.Slider("Weight", maker.diffuseWeight, 0f, 1f);
                        maker.diffuseRoughness = EditorGUILayout.Slider("Roughness", maker.diffuseRoughness, 0f, 1f);
                        maker.diffuseFresnel = EditorGUILayout.Toggle("Fresnel", maker.diffuseFresnel);

                        if(maker.diffuseFresnel)
                        {
                            EditorGUI.indentLevel++;
                            
                            maker.diffuseFresnelWeight = EditorGUILayout.Slider("Weight", maker.diffuseFresnelWeight, 0f, 1f);
                            maker.diffuseFresnelCurve = EditorGUILayout.CurveField("Curve", maker.diffuseFresnelCurve, GUILayout.MinHeight(50f));
                            EditorGUI.indentLevel--;
                        }

                        EditorGUI.indentLevel--;
                    }
                }

                // Specular
                {
                    if (GUILayout.Button("Specular", "Foldout"))
                        specularFoldout = !specularFoldout;

                    if (specularFoldout)
                    {
                        EditorGUI.indentLevel++;
                        maker.specularColor = EditorGUILayout.ColorField("Specular", maker.specularColor);
                        maker.specularWeight = EditorGUILayout.Slider("Weight", maker.specularWeight, 0f, 1f);
                        maker.specularRoughness = EditorGUILayout.Slider("Roughness", maker.specularRoughness, 0f, 1f);
                        maker.specularFresnel = EditorGUILayout.Toggle("Fresnel", maker.specularFresnel);

                        if (maker.specularFresnel)
                        {
                            EditorGUI.indentLevel++;
                            maker.specularFresnelWeight = EditorGUILayout.Slider("Weight", maker.specularFresnelWeight, 0f, 1f);
                            maker.specularFresnelCurve = EditorGUILayout.CurveField("Curve", maker.specularFresnelCurve, GUILayout.MinHeight(50f));
                            EditorGUI.indentLevel--;
                        }

                        EditorGUI.indentLevel--;
                    }
                }

                // Emissive
                {
                    if (GUILayout.Button("Emissive", "Foldout"))
                        emissiveFoldout = !emissiveFoldout;

                    if (emissiveFoldout)
                    {
                        EditorGUI.indentLevel++;
                        maker.emissiveColor = EditorGUILayout.ColorField("Emissive", maker.emissiveColor);
                        maker.emissiveWeight = EditorGUILayout.Slider("Weight", maker.emissiveWeight, 0f, 1f);
                        maker.emissiveFresnel = EditorGUILayout.Toggle("Fresnel", maker.emissiveFresnel);

                        if (maker.emissiveFresnel)
                        {
                            EditorGUI.indentLevel++;
                            maker.emissiveFresnelWeight = EditorGUILayout.Slider("Weight", maker.emissiveFresnelWeight, 0f, 1f);
                            maker.emissiveFresnelCurve = EditorGUILayout.CurveField("Curve", maker.emissiveFresnelCurve, GUILayout.MinHeight(50f));
                            EditorGUI.indentLevel--;
                        }

                        EditorGUI.indentLevel--;
                    }
                }

                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
            }

            // Lights
            {
                GUILayout.Space(20f);
                GUILayout.BeginVertical("Lights", "Window");

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("+", GUILayout.MaxWidth(25f), GUILayout.MinHeight(15f)))
                {
                    SMLight l = null;

                    switch (lightType)
                    {
                        case LightType.Point:
                            l = maker.gameObject.AddComponent<SMLight>();
                            break;
                        case LightType.Directional:
                            l = maker.gameObject.AddComponent<SMDirLight>();
                            break;
                        case LightType.Sphere:
                            l = maker.gameObject.AddComponent<SMSphereLight>();
                            break;
                    }

                    l.theta = Random.value * Mathf.PI * 2f;
                    l.phi = Random.value * Mathf.PI;

                    EditorUtility.SetDirty(target);
                }

                lightType = (LightType)EditorGUILayout.EnumPopup(lightType);

                GUILayout.EndHorizontal();

                if (GUILayout.Button("Remove all lights"))
                {
                    SMLight[] lights = maker.GetComponents<SMLight>();

                    foreach (SMLight l in lights)
                        GameObject.DestroyImmediate(l);
                }

                GUILayout.EndVertical();
            }
        }


        if (GUI.changed)
        {
            maker.BuildPreviewTexture();
            //Undo.RecordObject(target, "Matcap");
        }


        GUILayout.Space(5f);

        if (GUILayout.Button("Build"))
        {
            maker.BuildTexture();
        }

        if (GUILayout.Button("Save"))
        {
            Texture2D t = maker.GetTexture();
            byte[] bytes = t.EncodeToPNG();

            if (!System.IO.Directory.Exists(Application.dataPath + "/SphereMaps"))
                System.IO.Directory.CreateDirectory(Application.dataPath + "/SphereMaps");

            if (bytes != null)
                System.IO.File.WriteAllBytes(Application.dataPath + "/SphereMaps/" + maker.name + ".png", bytes);

            AssetDatabase.Refresh();
        }

        if(GUILayout.Button("Debug"))
        {
            //EditorGUI.ProgressBar()
            //EditorUtility.ClearProgressBar();
         //   EditorUtility.DisplayCancelableProgressBar("Waiting", "Rendering...", .5f);
        }

        GUILayout.EndVertical();


        // Dirty (heh) trick to get an update interval!
        EditorUtility.SetDirty(target);
    }

    
}
