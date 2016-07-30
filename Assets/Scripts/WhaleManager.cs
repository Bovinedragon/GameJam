using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WhaleManager : MonoBehaviour {

	public GameObject m_Whale;
	public TerrainBuilder m_TerrainBuilder;

	private const int c_num_whales = 5;
	private const int c_whale_y = 1;
	private const int c_map_width = 180;
	private const int c_map_height = 130;

	private List<GameObject> m_whaleList = new List<GameObject>();

	// Use this for initialization
	void Start () {

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

	void SpawnWhales () {
		const float minDist = 20.0f;
		const float minDistSqr = minDist * minDist;

		if (m_TerrainBuilder == null || m_TerrainBuilder.HeightDataReady() || m_whaleList.Count >= c_num_whales)
			return;

		for (int i = 0; i < c_num_whales; ++i) {
			Vector3 pos;
			do {
				pos = new Vector3(
					Random.Range(-c_map_width / 2, c_map_width / 2), 
					c_whale_y, 
					Random.Range(-c_map_height / 2, c_map_height / 2)
				);
			}
			while (m_TerrainBuilder.SampleHeightDataWorld(pos.x, pos.z) > 60 || ClosestWhaleDistSqr(pos) < minDistSqr);

			GameObject whale = GameObject.Instantiate(m_Whale);
			whale.transform.SetParent(transform);
			whale.transform.position = pos;
			m_whaleList.Add(whale);
		}
	}

	// Update is called once per frame
	void Update () {
		SpawnWhales();
	}
}
