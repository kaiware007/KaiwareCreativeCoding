using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Renderer))]
public class RaymarchingObject : MonoBehaviour {

    public Material material;

    private int scaleID;

	// Use this for initialization
	void Start () {
        GetComponent<Renderer>().material = material;
        scaleID = Shader.PropertyToID("_Scale");

    }
	
	// Update is called once per frame
	void Update () {
        material.SetVector(scaleID, transform.localScale);
    }
}
