using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour {

    public int score = 0;
    public Text output;
    public float starVal;

	// Update is called once per frame
	void Update () {
        output.text = "SCORE: " + score;
	}

    void AddScore(float gmeter)
    {
        score = score + Mathf.FloorToInt(starVal * gmeter);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.Contains("Star"))
        {
            AddScore(this.GetComponent<GrooveMeter>().grove);
            Destroy(col.gameObject);
        }
    }
}
