﻿using UnityEngine;
using System.Collections;

public class Whale : MonoBehaviour {

	public int m_StartHealth = 15;
	public int m_MaxHealth = 30;

	private int m_currentHealth;

	public void Damage () {
		if (m_currentHealth > 0)
			m_currentHealth--;
	}

	public void Heal () {
		if (m_currentHealth < m_MaxHealth)
			m_currentHealth++;
	}

	// Use this for initialization
	void Start () {
		m_currentHealth = m_StartHealth;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter (Collider other) {
		if (other.transform.parent != null && other.transform.parent.name == "FishManager") {
			Destroy(other.gameObject);
			Heal();
		}
	}
		
}
