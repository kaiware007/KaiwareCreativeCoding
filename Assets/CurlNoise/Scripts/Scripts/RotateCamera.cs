using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour {
    public Vector3 target;
    public Vector3 up = Vector3.up;
    public float rotateSpeed = 1;

	// Update is called once per frame
	void Update () {
        transform.RotateAround(target, up, rotateSpeed * Time.deltaTime);
	}
}
