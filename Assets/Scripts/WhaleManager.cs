using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WhaleManager : MonoBehaviour {

	public GameObject m_Whale;
	public TerrainBuilder m_TerrainBuilder;
    public AudioClip m_WhaleHappySound;
    public AudioClip m_WhaleDeadSound;
    public float m_WhaleVolume = 0.5f;

	public int m_MaxWhales = 3;
	public float m_StartSpawnDelay = 2.0f;
	public float m_SpawnDelay = 3.0f;

	private float m_spawnTimer;

	public int m_WhalesKilled = 0;
	public int m_WhalesFullyFed = 0;

	public Text m_DeadText;
	public Text m_HappyText;

	private const int c_whale_y = 1;
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
        SoundManager.Get().PlayOneShotSound(m_WhaleDeadSound, m_WhaleVolume, whale.transform.position);
		m_whaleList.Remove(whale);
		DestroyObject(whale);
        if (GameManager.Get() != null)
            GameManager.Get().UpdateGameData(m_WhalesFullyFed, m_WhalesKilled);
	}

	public void WhaleFullyFed (GameObject whale) {
		m_WhalesFullyFed++;
		m_HappyText.text = m_WhalesFullyFed.ToString();
        SoundManager.Get().PlayOneShotSound(m_WhaleHappySound, m_WhaleVolume, whale.transform.position);
		m_whaleList.Remove(whale);
		DestroyObject(whale);
        if (GameManager.Get() != null)
            GameManager.Get().UpdateGameData(m_WhalesFullyFed, m_WhalesKilled);
	}

	void SpawnWhale () {
		const float minDist = 40.0f;
		const float minDistSqr = minDist * minDist;

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
