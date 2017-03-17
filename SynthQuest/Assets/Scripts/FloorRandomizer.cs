using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorRandomizer : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (this.transform.localEulerAngles.z >=90 || this.transform.localEulerAngles.z <=-90)
            this.transform.localEulerAngles = new Vector3(Random.Range(0, 4) * 90, 0, this.transform.localEulerAngles.z);
        else 
            this.transform.localEulerAngles = new Vector3(0,Random.Range(0,4)*90, 0);
	}
	
}
