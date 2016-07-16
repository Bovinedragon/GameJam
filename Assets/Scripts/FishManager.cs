using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishManager : MonoBehaviour {

	public GameObject m_Fish;

	private const int c_num_fish = 40;
	private const int c_fish_y = 1;
	private const int c_map_width = 180;
	private const int c_map_height = 130;
	private const float c_fish_speed = 5.0f;

	class FishData {
		public GameObject m_fish;
		public Vector3 m_velocity;
	};

	private List<FishData> m_fishList = new List<FishData>();

	// Use this for initialization
	void Start () {
		for (int i = 0; i < c_num_fish; ++i) {
			Vector3 pos = new Vector3(
				Random.Range(-c_map_width / 2, c_map_width / 2), 
				c_fish_y, 
				Random.Range(-c_map_height / 2, c_map_height / 2)
			);

			float angle = Random.Range(0, 2 * Mathf.PI);
			Vector3 vel = new Vector3(
				Mathf.Cos(angle) * c_fish_speed,
				0.0f,
				Mathf.Sin(angle) * c_fish_speed
			);

			GameObject fish = GameObject.Instantiate (m_Fish);
			fish.transform.position = pos;

			FishData fishData = new FishData();
			fishData.m_fish = fish;
			fishData.m_velocity = vel;
			m_fishList.Add(fishData);
		}
	}
	
	// Update is called once per frame
	void Update () {
		foreach (FishData fish in m_fishList) {
			fish.m_fish.transform.position += fish.m_velocity * Time.deltaTime;

			if (fish.m_velocity.x < 0 && fish.m_fish.transform.position.x < -c_map_width / 2) {
				fish.m_velocity.x *= -1.0f;
			} else if (fish.m_velocity.x > 0 && fish.m_fish.transform.position.x > c_map_width / 2) {
				fish.m_velocity.x *= -1.0f;
			}

			if (fish.m_velocity.z < 0 && fish.m_fish.transform.position.z < -c_map_height / 2) {
				fish.m_velocity.z *= -1.0f;
			} else if (fish.m_velocity.z > 0 && fish.m_fish.transform.position.z > c_map_height / 2) {
				fish.m_velocity.z *= -1.0f;
			}
		}
	}
}
