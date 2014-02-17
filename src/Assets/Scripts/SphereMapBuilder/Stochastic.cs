using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Stochastic  
{    
	public static List<Vector2> GetStratifiedUniformGrid(int samples)
    {
        int subdiv = (int)Mathf.Sqrt(samples);
        return GetStratifiedUniformGrid(subdiv, subdiv);
	}

    public static List<Vector2> GetStratifiedUniformGrid(int samplesX, int samplesY)
    {
        List<Vector2> res = new List<Vector2>();

        float gridSizeX = 1.0f / samplesX;
        float gridSizeY = 1.0f / samplesY;

        for (int i = 0; i < samplesX; i++)
            for (int j = 0; j < samplesY; j++)
            {
                float u = (i + Random.value) * gridSizeX;
                float v = (j + Random.value) * gridSizeY;

                res.Add(new Vector2(u, v));
            }

        return res;
    }

    public static Vector3 GetRandomHemisphereDirection(Vector3 normal)
    {
        Vector3 tangent = Vector3.zero;
        Vector3 binormal = Vector3.zero;

        Vector3.OrthoNormalize(ref normal, ref tangent, ref binormal);

        float theta = Random.value * 2 * Mathf.PI;
        float phi = Random.value * Mathf.PI / 2;

        float x = Mathf.Cos(theta) * Mathf.Sin(phi);
        float y = Mathf.Sin(theta) * Mathf.Sin(phi);
        float z = Mathf.Cos(phi);

        return tangent * x + binormal * y + normal * z;
    }
}
