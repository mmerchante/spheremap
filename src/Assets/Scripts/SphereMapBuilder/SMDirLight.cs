using UnityEngine;
using System.Collections;

public class SMDirLight : SMLight
{
    public override Vector3 SampleLightIncidence(Vector3 fromPoint)
    {
        return Position.normalized;
    }
}
