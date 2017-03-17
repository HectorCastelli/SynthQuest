using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour {

    public Transform target;
    public float velocity = 0.5f;


    private Vector3 distance;

	// Use this for initialization
	void Start () {
        distance = this.transform.position - target.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 tarPos = target.position + distance;
        this.transform.position = Vector3.Lerp(this.transform.position, tarPos, velocity * Time.deltaTime);
	}
}
