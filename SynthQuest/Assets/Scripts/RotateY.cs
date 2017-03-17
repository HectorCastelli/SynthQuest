using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateY : MonoBehaviour {

    public float velocity;

	// Update is called once per frame
	void Update () {
        this.transform.localEulerAngles = new Vector3(0, this.transform.localEulerAngles.y + velocity, 0);
	}
}
