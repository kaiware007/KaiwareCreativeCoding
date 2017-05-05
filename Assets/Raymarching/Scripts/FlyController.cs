using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyController : MonoBehaviour {

    public float maxSpeed = 10;
    public float speed = 0;
    public float accel = 1;
    public float rotSpeed = 1;

    public bool isDebug = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float dt = Time.deltaTime;
        float vx = Input.GetAxis("Horizontal");
        float vy = Input.GetAxis("Vertical");

        if (isDebug)
        {
            vx = 1;
        }

        vx *= rotSpeed * dt;
        vy *= rotSpeed * dt;

        transform.Rotate(vy, vx, 0);

        if (Input.GetKey(KeyCode.LeftShift) || isDebug)
        {
            speed = Mathf.Min(speed + accel * dt, maxSpeed);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            speed = Mathf.Max(speed - accel * dt, -maxSpeed);
        }

        transform.Translate(0, 0, speed * dt);
    }
}
