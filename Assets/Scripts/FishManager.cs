using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishManager : MonoBehaviour {

	public GameObject m_Fish;

	private const int c_num_fish = 80;
	private const int c_fish_y = 1;
	private const float c_fish_speed = 5.0f;

	private Rect c_map_bounds = new Rect(-100.0f, -100.0f, 200.0f, 200.0f);
	private Rect c_fish_bounds = new Rect(-90.0f, -65.0f, 180.0f, 130.0f);

	class FishData {
		public GameObject m_fish;
		public Vector3 m_velocity;
	};

	private List<FishData> m_fishList = new List<FishData>();

	private Color32[] m_terrainData;
	private int m_terrainWidth;
	private int m_terrainHeight;

	private float[] m_vectorField;
	private int m_vectorFieldWidth;
	private int m_vectorFieldHeight;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < c_num_fish; ++i) {
			Vector3 pos = new Vector3(
				Random.Range(c_fish_bounds.xMin, c_fish_bounds.xMax), 
				c_fish_y, 
				Random.Range(c_fish_bounds.yMin, c_fish_bounds.yMax)
			);

			float angle = Random.Range(0, 2 * Mathf.PI);
			Vector3 vel = new Vector3(
				Mathf.Cos(angle) * c_fish_speed,
				0.0f,
				Mathf.Sin(angle) * c_fish_speed
			);

			GameObject fish = GameObject.Instantiate(m_Fish);
			fish.transform.position = pos;

			FishData fishData = new FishData();
			fishData.m_fish = fish;
			fishData.m_velocity = vel;
			m_fishList.Add(fishData);
		}
	}

	public void SetHeightTexture (ProceduralTexture texture) {
		m_terrainData = texture.GetPixels32(0, 0, texture.width, texture.height);
		m_terrainWidth = texture.width;
		m_terrainHeight = texture.height;
	}

	public void SetVectorField (float[] vectorField, int width, int height) {
		m_vectorField = vectorField;
		m_vectorFieldWidth = width;
		m_vectorFieldHeight = height;
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
			if (i == fish)
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
			if (i == fish)
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

	int SampleTerrainData (float x, float y) {
		int tx = (int)(((x - c_map_bounds.xMin) / c_map_bounds.width) * m_terrainWidth);
		int ty = (int)(((y - c_map_bounds.yMin) / c_map_bounds.height) * m_terrainHeight);
		tx = Mathf.Clamp(tx, 0, m_terrainWidth - 1);
		ty = Mathf.Clamp(ty, 0, m_terrainHeight - 1);
		return m_terrainData[tx + ty * m_terrainHeight].r;
	}

	Vector3 AvoidTerrain (int fish) {
		if (m_terrainData == null)
			return Vector3.zero;
		
		Vector3 pos = m_fishList[fish].m_fish.transform.position;

		int height = SampleTerrainData(pos.x, pos.z);
		if (height < 128)
			return Vector3.zero;

		float offset = 3.0f;
		int r = SampleTerrainData(pos.x + offset, pos.z);
		int l = SampleTerrainData(pos.x - offset, pos.z);
		int u = SampleTerrainData(pos.x, pos.z + offset);
		int d = SampleTerrainData(pos.x, pos.z - offset);
		Vector3 vx = new Vector3(offset, r - l, 0.0f);
		Vector3 vz = new Vector3(0.0f, u - d, offset);
		Vector3 normal = Vector3.Cross(vz, vx);
		normal.y = 0.0f;

		return normal;
	}

	Vector2 SampleVectorField (float x, float y) {
		int tx = (int)(((x - c_map_bounds.xMin) / c_map_bounds.width) * m_vectorFieldWidth);
		int ty = (int)(((y - c_map_bounds.yMin) / c_map_bounds.height) * m_vectorFieldHeight);
		tx = Mathf.Clamp(tx, 0, m_vectorFieldWidth - 1);
		ty = Mathf.Clamp(ty, 0, m_vectorFieldHeight - 1);

		float len = m_vectorField[tx + ty * m_vectorFieldWidth + 0];
		float vx = m_vectorField[tx + ty * m_vectorFieldWidth + 1];
		float vy = m_vectorField[tx + ty * m_vectorFieldWidth + 2];

		return new Vector3(vx * len, 0.0f, vy * len);
	}
		
	Vector3 FollowVectorField (int fish) {
		if (m_vectorField == null)
			return Vector3.zero;

		Vector3 pos = m_fishList[fish].m_fish.transform.position;
		return SampleVectorField(pos.x, pos.y);
	}

	void Flock () {
		const float c_cruising_weight = 1.0f;
		const float c_keep_distance_weight = 0.0f;
		const float c_watch_heading_weight = 0.0f;
		const float c_avoid_edge_weight = 50.0f;
		const float c_avoid_terrain_weight = 50.0f;
		const float c_follow_vector_field_weight = 10.0f;

		for (int i = 0; i < m_fishList.Count; ++i) {
			Vector3 newHeading = Vector3.zero;
			newHeading = newHeading + Cruising(i).normalized * c_cruising_weight;
			newHeading = newHeading + KeepDistance(i).normalized * c_keep_distance_weight;
			newHeading = newHeading + WatchHeading(i).normalized * c_watch_heading_weight;
			newHeading = newHeading + AvoidEdge(i).normalized * c_avoid_edge_weight;
			newHeading = newHeading + AvoidTerrain(i).normalized * c_avoid_terrain_weight;
			newHeading = newHeading + FollowVectorField(i).normalized * c_follow_vector_field_weight;

			m_fishList[i].m_velocity = newHeading.normalized * c_fish_speed;
		}
	}

	void Step () {
		foreach (var fish in m_fishList) {
			if (fish.m_fish == null)
				continue;

			fish.m_fish.transform.position += fish.m_velocity * Time.deltaTime;
		}
	}

	// Update is called once per frame
	void Update () {
		Flock();
		Step();
	}

}
