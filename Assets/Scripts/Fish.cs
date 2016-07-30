using UnityEngine;
using System.Collections;

public class Fish : MonoBehaviour {

	private Vector3 m_velocity;

	public void Eat () {
		FishManager manager = GetComponentInParent<FishManager>();
		manager.Eat(this.gameObject);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
