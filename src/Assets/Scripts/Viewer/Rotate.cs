using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour 
{

	public void Update () 
	{
        this.transform.rotation = Quaternion.Euler(Vector3.up * Time.time * 25f);
	}
}
