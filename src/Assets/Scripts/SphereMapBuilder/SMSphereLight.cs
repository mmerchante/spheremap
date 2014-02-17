using UnityEngine;
using System.Collections;

public class SMSphereLight : SMLight 
{
    public float radius = 1f;

    public override Vector3 SampleLightIncidence(Vector3 fromPoint)
    {
        Vector3 normal = (fromPoint - Position).normalized;
        return (fromPoint - (Stochastic.GetRandomHemisphereDirection(normal) * radius + Position)).normalized;
    }

    public override int MaxSamples()
    {
        return 1;
    }

    public override float Area()
    {
        // Area of hemisphere only (what we really sample)
        return 4f * Mathf.PI * radius * radius;
    }

    public override bool CheckCollision(Vector3 rPos, Vector3 rDir)
    {
        rDir *= -1f;
        rPos -= Position;
       
        float b = 2 * Vector3.Dot(rPos, rDir);
        float c = rPos.sqrMagnitude - radius * radius;

        float discriminant = b * b - 4 * c;

        if (discriminant < 0)
        {
            return false;
        }

        discriminant = Mathf.Sqrt(discriminant);

        double t1 = (-b - discriminant) / 2;

        return t1 > 0;
    }
}
