using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WhaleManager : MonoBehaviour {

	public GameObject m_Whale;

	private const int c_num_whales = 5;
	private const int c_whale_y = 1;
	private const int c_map_width = 180;
	private const int c_map_height = 130;

	private List<GameObject> m_whaleList = new List<GameObject>();

	// Use this for initialization
	void Start () {
		for (int i = 0; i < c_num_whales; ++i) {
			Vector3 pos = new Vector3(
				Random.Range(-c_map_width / 2, c_map_width / 2), 
				c_whale_y, 
				Random.Range(-c_map_height / 2, c_map_height / 2)
			);

			GameObject whale = GameObject.Instantiate(m_Whale);
			whale.transform.SetParent(transform);
			whale.transform.position = pos;
			m_whaleList.Add(whale);
		}
	
	}
		
	// Update is called once per frame
	void Update () {
	
	}
}
