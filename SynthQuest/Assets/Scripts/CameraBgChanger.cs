using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBgChanger : MonoBehaviour {
    public bool right = true;

    public Color color1;
    public Color color2;
    public float speed;

    public float progress = 0;
	// Use this for initialization
	void Start () {
        this.GetComponent<Camera>().backgroundColor = color1;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (progress > 1 && right)
            right = false;
        else if (progress < 0 && !right)
        {
            right = true;
        }

        if (right)
        {
            progress += Time.deltaTime * speed;
        } else
        {
            progress -= Time.deltaTime * speed;
        }
        this.GetComponent<Camera>().backgroundColor = Color.Lerp(color1, color2, progress);
	}
}
