using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseDetector : MonoBehaviour {


    public GameObject player;
    public float offset = 5f;
    public float high;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (player.transform.position.y > high)
            high = player.transform.position.y;

        this.transform.position = new Vector3(10.8f, high-offset, 0);
	}

    public void Lose()
    {
        Debug.Log("You lost");
    }

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player")
        {
            Lose();
        }
    }
}
