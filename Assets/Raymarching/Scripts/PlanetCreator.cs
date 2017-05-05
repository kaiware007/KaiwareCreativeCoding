using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetCreator : MonoBehaviour {

    public float spaceRadius = 10000;
    public int planetNum = 10;
    public float maxSize = 100;
    public float minSize = 10;

	// Use this for initialization
	void Start () {
		for(int i = 0; i < planetNum; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.position = Random.insideUnitSphere * spaceRadius;
            obj.transform.localScale = Vector3.one * Random.Range(minSize, maxSize);
        }
	}

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, spaceRadius);
    }
}
