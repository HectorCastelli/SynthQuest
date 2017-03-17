using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrooveMeter : MonoBehaviour {

    public float score = 0;
    public float sum = 0;
    private int count;

    public Image progress;

	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        count++;
        sum += this.GetComponent<Rigidbody>().velocity.magnitude;
        score = sum / count;

        progress.fillAmount = score / 10f;
	}
}
