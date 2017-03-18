using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPoint : MonoBehaviour {

    public GamePlacer gplacer;

    void Start()
    {
        this.GetComponent<MeshRenderer>().enabled = false;
        gplacer = GameObject.Find("GamePlacer").GetComponent<GamePlacer>();
    }

	void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player")
        {
            gplacer.EnableLoad();
            Destroy(this.gameObject);
        }
    }
}
