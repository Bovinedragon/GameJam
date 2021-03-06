﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WhaleManager : MonoBehaviour {

	public GameObject m_Whale;
	public TerrainBuilder m_TerrainBuilder;
	public CBoatManager m_BoatManager;

	public int m_MaxWhales = 3;
	public float m_StartSpawnDelay = 2.0f;
	public float m_SpawnDelay = 3.0f;

	private float m_spawnTimer;

	public int m_WhalesKilled = 0;
	public int m_WhalesFullyFed = 0;

	public Text m_DeadText;
	public Text m_HappyText;

	private const float c_whale_y = 1.0f;
	private const int c_map_width = 180;
	private const int c_map_height = 130;

	private List<GameObject> m_whaleList = new List<GameObject>();

	// Use this for initialization
	void Start () {
		m_spawnTimer = m_StartSpawnDelay;
	}

	float ClosestWhaleDistSqr (Vector3 pos) {
		float closestSqrMag = 999999.0f;
		for (int i = 0; i < m_whaleList.Count; ++i) {
			Vector3 diff = pos - m_whaleList[i].transform.position;
			float sqrMag = diff.sqrMagnitude;
			if (sqrMag < closestSqrMag) {
				closestSqrMag = sqrMag;
			}
		}

		return closestSqrMag;
	}

    public void EnumerateWhalesInRange (Vector3 pos, float radius, out List<GameObject> whales)
    {
        whales = new List<GameObject>();
        float radiusSq = radius * radius;
        foreach (GameObject whale in m_whaleList)
        {
            Vector3 delta = pos - whale.transform.position;
            if (delta.sqrMagnitude < radiusSq)
                whales.Add(whale);
        }
    }
		
	public void WhaleKilled (GameObject whale) {
		m_WhalesKilled++;
		m_DeadText.text = m_WhalesKilled.ToString();
		m_whaleList.Remove(whale);
		DestroyObject(whale);
        if (GameManager.Get() != null)
            GameManager.Get().UpdateGameData(m_WhalesFullyFed, m_WhalesKilled);
	}

	public void WhaleFullyFed (GameObject whale) {
		m_WhalesFullyFed++;
		m_HappyText.text = m_WhalesFullyFed.ToString();
		m_whaleList.Remove(whale);
		DestroyObject(whale);
        if (GameManager.Get() != null)
            GameManager.Get().UpdateGameData(m_WhalesFullyFed, m_WhalesKilled);
	}

	void SpawnWhale () {
		const float minWhaleDist = 40.0f;
		const float minBoatDist = 40.0f;
		const float minWhaleDistSqr = minWhaleDist * minWhaleDist;
		int maxTryCount = 30;
		int tryCount = 0;

		Vector3 pos;
		Vector3 posRight;
		Vector3 posLeft;
		Vector3 posUp;
		Vector3 posDown;
		do {
			if (tryCount > maxTryCount)
				return;
			
			pos = new Vector3(
				Random.Range(-c_map_width / 2, c_map_width / 2), 
				c_whale_y, 
				Random.Range(-c_map_height / 2, c_map_height / 2)
			);
			tryCount++;

			posRight = pos + new Vector3(10.0f, 0.0f, 0.0f);
			posLeft = pos + new Vector3(-4.0f, 0.0f, 0.0f);
			posUp = pos + new Vector3(0.0f, 0.0f, 5.0f);
			posDown = pos + new Vector3(0.0f, 0.0f, -5.0f);
		}
		while (
			m_TerrainBuilder.SampleHeightDataWorld(pos.x, pos.z) > 60 
			|| m_TerrainBuilder.SampleHeightDataWorld(posRight.x, posRight.z) > 60 
			|| m_TerrainBuilder.SampleHeightDataWorld(posLeft.x, posLeft.z) > 60 
			|| m_TerrainBuilder.SampleHeightDataWorld(posUp.x, posUp.z) > 60 
			|| m_TerrainBuilder.SampleHeightDataWorld(posDown.x, posDown.z) > 60 
			|| ClosestWhaleDistSqr(pos) < minWhaleDistSqr
			|| m_BoatManager.BoatInRegion(pos, minBoatDist)
		);

		GameObject whale = GameObject.Instantiate(m_Whale);
		whale.transform.SetParent(transform);
		whale.transform.position = pos;
		whale.transform.LookAt(pos + new Vector3(1.0f, 0.0f, 0.0f));

		Whale whaleComp = whale.GetComponent<Whale>();
		whaleComp.m_SpawnLocation = pos;

		m_whaleList.Add(whale);
	}

	// Update is called once per frame
	void Update () {
		if (m_TerrainBuilder == null || !m_TerrainBuilder.HeightDataReady())
			return;
		
		if (m_whaleList.Count == m_MaxWhales)
			return;
		
		m_spawnTimer -= Time.deltaTime;
		if (m_spawnTimer <= 0.0f) {
			SpawnWhale();
			m_spawnTimer = m_SpawnDelay;
		}
	}
}
