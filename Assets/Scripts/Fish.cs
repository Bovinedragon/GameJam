using UnityEngine;
using System.Collections;

public class Fish : MonoBehaviour {

	public float m_IntroTime = 0.25f;

	private Vector3 m_velocity;
	private float m_AnimTime;

	public void Eat () {
		FishManager manager = GetComponentInParent<FishManager>();
		manager.Eat(this.gameObject);
	}

	void UpdateAnim () {
		if (m_AnimTime < m_IntroTime) {
			transform.localScale = Vector3.one * (m_AnimTime / m_IntroTime);
		} else {
			transform.localScale = Vector3.one;
		}
	}

	// Use this for initialization
	void Start () {
		UpdateAnim();
	}
	
	// Update is called once per frame
	void Update () {
		m_AnimTime += Time.deltaTime;
		UpdateAnim();
	}
}
