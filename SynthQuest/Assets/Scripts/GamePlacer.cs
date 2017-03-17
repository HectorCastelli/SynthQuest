using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlacer : MonoBehaviour {

    public GameObject[] mapParts;

    public float currentHeight = 7.2f;

	// Use this for initialization
	void Start () {
		
	}
	
    public void EnableLoad()
    {
        GameObject level = Instantiate(mapParts[Random.Range(0, mapParts.Length)], this.transform);
        level.GetComponentInChildren<MapPoint>().gplacer = this.GetComponent<GamePlacer>();
        level.transform.position = new Vector3(0, currentHeight, 0);
        currentHeight += CalculateLocalBounds(level).size.y;
    }


    private Bounds CalculateLocalBounds(GameObject go)
    {
        Quaternion currentRotation = go.transform.rotation;
        go.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Bounds bounds = new Bounds(go.transform.position, Vector3.zero);

        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }

        Vector3 localCenter = bounds.center - go.transform.position;
        bounds.center = localCenter;

        go.transform.rotation = currentRotation;

        return bounds;
    }
}
