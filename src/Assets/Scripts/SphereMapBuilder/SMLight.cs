using UnityEngine;
using System.Collections;

public class SMLight : MonoBehaviour 
{
    public float distToCenter = 5f;
    public float theta;
    public float phi;

    public float intensity = 1f;
    public Color color = Color.gray;

    public void OnEnable() { hideFlags = HideFlags.HideAndDontSave; }

    public Vector3 Position
    {
        get { return new Vector3(Mathf.Cos(theta) * Mathf.Sin(phi), Mathf.Cos(phi), Mathf.Sin(theta) * Mathf.Sin(phi)) * distToCenter; }
    }

    public virtual int MaxSamples()
    {
        return 1;
    }

    public virtual Vector3 SampleLightIncidence(Vector3 fromPoint)
    {
        return (fromPoint - Position).normalized;
    }

    public virtual bool CheckCollision(Vector3 from, Vector3 dir)
    {
        return false;
    }

    public virtual float Area()
    {
        return 1f;
    }
}
