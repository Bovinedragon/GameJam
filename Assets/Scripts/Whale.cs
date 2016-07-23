using UnityEngine;
using System.Collections;

public class Whale : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter (Collider other) {
		if (other.transform.parent != null)
			Destroy(other.gameObject);
	}
		
}
