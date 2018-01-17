using UnityEngine;
using System.Collections;

public class DestoryThisTimed : MonoBehaviour 
{
    public float destroyTime;

	void Start () 
    {
        GameObject.Destroy(this.gameObject, destroyTime);
	}
	
	void Update () 
    {
	
	}
}
