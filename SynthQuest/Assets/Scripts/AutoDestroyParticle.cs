using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyParticle : MonoBehaviour {
	void Start () {
        Destroy(this.gameObject, this.GetComponent<ParticleSystem>().main.duration);
	}
}
