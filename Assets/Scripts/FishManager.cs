using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishManager : MonoBehaviour {

	public GameObject m_Fish;
	public CWaterSimulation m_WaterSimulation;
	public TerrainBuilder m_TerrainBuilder;

	public int m_MaxFish = 80;

	public float m_StartSpawnDelay = 2.0f;
	public float m_SpawnDelay = 0.3f;

	private float m_spawnTimer;

	private const float c_fish_y = 1.0f;
	private const float c_fish_speed = 5.0f;

	//private Rect c_map_bounds = new Rect(-100.0f, -100.0f, 200.0f, 200.0f);
	private Rect c_fish_bounds = new Rect(-90.0f, -65.0f, 180.0f, 130.0f);

	class FishData {
		public GameObject m_fish;
		public Vector3 m_velocity;
	};

	private List<FishData> m_fishList = new List<FishData>();

	public void Eat (GameObject fish) {
		for (int i = 0; i < m_fishList.Count; ++i) {
			if (fish == m_fishList[i].m_fish) {
				m_fishList.RemoveAt(i);				
				DestroyObject(fish);
				return;
			}
		}
	}

	void SpawnFish () {
		Vector3 pos;
		do {
			pos = new Vector3(
				Random.Range(c_fish_bounds.xMin, c_fish_bounds.xMax), 
				c_fish_y, 
				Random.Range(c_fish_bounds.yMin, c_fish_bounds.yMax)
			);
		}
		while (m_TerrainBuilder.SampleHeightDataWorld(pos.x, pos.z) > 110);

		float angle = Random.Range(0, 2 * Mathf.PI);
		Vector3 vel = new Vector3(
			Mathf.Cos(angle) * c_fish_speed,
			0.0f,
			Mathf.Sin(angle) * c_fish_speed
		);

		GameObject fish = GameObject.Instantiate(m_Fish);
		fish.transform.SetParent(transform);
		fish.transform.position = pos;

		FishData fishData = new FishData();
		fishData.m_fish = fish;
		fishData.m_velocity = vel;
		m_fishList.Add(fishData);
	}

	// Use this for initialization
	void Start () {
		m_spawnTimer = m_StartSpawnDelay;
	}

	Vector3 Cruising (int fish) {
		return m_fishList[fish].m_velocity;
	}

	Vector3 KeepDistance (int fish) {
		const float visionDist = 4.0f;
		const float avoidDist = 4.0f;
		const float visionDistSqr = visionDist * visionDist;
		const float avoidDistSqr = avoidDist * avoidDist;

		Vector3 pos = m_fishList[fish].m_fish.transform.position;

		Vector3 avoid = Vector3.zero;
		float closestSqrMag = 999999.0f;
		for (int i = 0; i < m_fishList.Count; ++i) {
			if (i == fish || m_fishList[i].m_fish == null)
				continue;
			
			Vector3 diff = pos - m_fishList[i].m_fish.transform.position;
			float sqrMag = diff.sqrMagnitude;
			if (sqrMag < visionDistSqr && sqrMag < closestSqrMag) {
				if (sqrMag < avoidDistSqr)
					avoid = diff;
				else
					avoid = diff * -1.0f;
				
				closestSqrMag = sqrMag;
			}
		}

		return avoid;
	}

	Vector3 WatchHeading (int fish) {
		const float visionDist = 10.0f;
		const float visionDistSqr = visionDist * visionDist;

		Vector3 pos = m_fishList[fish].m_fish.transform.position;

		Vector3 heading = Vector3.zero;
		for (int i = 0; i < m_fishList.Count; ++i) {
			if (i == fish || m_fishList[i].m_fish == null)
				continue;

			Vector3 diff = pos - m_fishList[i].m_fish.transform.position;
			float sqrMag = diff.sqrMagnitude;
			if (sqrMag < visionDistSqr) {
				heading += m_fishList[i].m_velocity.normalized;
			}
		}

		return heading;
	}

	Vector3 AvoidEdge (int fish) {
		Vector3 pos = m_fishList[fish].m_fish.transform.position;
		Vector3 vel = m_fishList[fish].m_velocity;

		Vector3 avoid = Vector3.zero;

		if (vel.x < 0 && pos.x < c_fish_bounds.xMin) {
			avoid.x = 1.0f;
		} else if (vel.x > 0 && pos.x > c_fish_bounds.xMax) {
			avoid.x = -1.0f;
		}

		if (vel.z < 0 && pos.z < c_fish_bounds.yMin) {
			avoid.z = 1.0f;
		} else if (vel.z > 0 && pos.z > c_fish_bounds.yMax) {
			avoid.z = -1.0f;
		}

		return avoid;
	}

	Vector3 AvoidTerrain (int fish) {
		if (m_TerrainBuilder == null)
			return Vector3.zero;

		Vector3 pos = m_fishList[fish].m_fish.transform.position;

		int height = m_TerrainBuilder.SampleHeightDataWorld(pos.x, pos.z);
		if (height < 110)
			return Vector3.zero;

		float offset = 3.0f;
		int r = m_TerrainBuilder.SampleHeightDataWorld(pos.x + offset, pos.z);
		int l = m_TerrainBuilder.SampleHeightDataWorld(pos.x - offset, pos.z);
		int u = m_TerrainBuilder.SampleHeightDataWorld(pos.x, pos.z + offset);
		int d = m_TerrainBuilder.SampleHeightDataWorld(pos.x, pos.z - offset);
		Vector3 vx = new Vector3(offset, r - l, 0.0f);
		Vector3 vz = new Vector3(0.0f, u - d, offset);
		Vector3 normal = Vector3.Cross(vz, vx);
		normal.y = 0.0f;

		return normal;
	}
		
	Vector3 FollowVectorField (int fish) {
		if (m_WaterSimulation == null)
			return Vector3.zero;

		Vector3 pos = m_fishList[fish].m_fish.transform.position;
		Vector2 vec = m_WaterSimulation.SampleVectorFieldWorld(pos.x, pos.z);
		return new Vector3(vec.x, 0.0f, vec.y);;
	}

	void Spawn () {
		if (m_fishList.Count == m_MaxFish || m_TerrainBuilder == null)
			return;

		m_spawnTimer -= Time.deltaTime;
		if (m_spawnTimer <= 0.0f) {
			SpawnFish();
			m_spawnTimer = m_SpawnDelay;
		}
	}

	void Flock () {
		const float c_cruising_weight = 10.0f;
		const float c_keep_distance_weight = 4.0f;
		const float c_watch_heading_weight = 4.0f;
		const float c_avoid_edge_weight = 40.0f;
		const float c_avoid_terrain_weight = 40.0f;
		const float c_follow_vector_field_weight = 7.0f;

		for (int i = 0; i < m_fishList.Count; ++i) {
			if (m_fishList[i].m_fish == null)
				continue;
			
			Vector3 newHeading = Vector3.zero;
			newHeading = newHeading + Cruising(i).normalized * c_cruising_weight;
			newHeading = newHeading + KeepDistance(i).normalized * c_keep_distance_weight;
			newHeading = newHeading + WatchHeading(i).normalized * c_watch_heading_weight;
			newHeading = newHeading + AvoidEdge(i).normalized * c_avoid_edge_weight;
			newHeading = newHeading + AvoidTerrain(i).normalized * c_avoid_terrain_weight;
			newHeading = newHeading + FollowVectorField(i).normalized * c_follow_vector_field_weight;

			newHeading = newHeading.normalized * c_fish_speed;

			float maxAngle = Time.deltaTime * Mathf.PI * 2.0f;
			float angle = Vector3.AngleBetween(m_fishList[i].m_velocity, newHeading);
			if (angle > maxAngle)
				newHeading = newHeading.normalized * c_fish_speed / 20;

			m_fishList[i].m_velocity = Vector3.RotateTowards(m_fishList[i].m_velocity, newHeading, maxAngle, 999.0f);
		}
	}

	void Step () {
		foreach (var fish in m_fishList) {
			fish.m_fish.transform.position += fish.m_velocity * Time.deltaTime;
			fish.m_fish.transform.LookAt(fish.m_fish.transform.position - fish.m_velocity);
		}
	}

	// Update is called once per frame
	void Update () {
		Spawn();
		Flock();
		Step();
	}

}
