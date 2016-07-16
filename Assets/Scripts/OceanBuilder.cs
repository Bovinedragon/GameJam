using UnityEngine;
using System.Collections;

public class OceanBuilder : MonoBehaviour
{
	public int m_Size = 128;
	public Vector3 m_Scale = Vector3.one;
	public Material m_OceanMaterial;

	private Mesh m_mesh;
	private Vector3[] m_vertices;
	
	void Awake()
	{
		BuildOceanMesh();
	}
	
	void Update()
	{
		for (int i=0; i < m_vertices.Length; i++)
		{
			Vector3 pos = m_vertices[i];
			m_vertices[i] = new Vector3(pos.x, Mathf.Cos((pos.x * 2) + Time.timeSinceLevelLoad) * m_Scale.y, pos.z);
		}
		m_mesh.vertices = m_vertices;
		m_mesh.RecalculateNormals();
	}

	private void BuildOceanMesh()
	{
		MeshFilter meshFilter = this.gameObject.AddComponent<MeshFilter>();
		m_mesh = new Mesh();
		meshFilter.mesh = m_mesh;

		int width = m_Size-1;
		int height = m_Size-1;

		// build mesh
		m_vertices = new Vector3[(width + 1) * (height + 1)];
		Vector2[] uv = new Vector2[m_vertices.Length];
		Vector4[] tangents = new Vector4[m_vertices.Length];
        Vector4 t = new Vector4(1f, 0f, 0f, -1f);
		for (int i = 0, z = 0; z <= height; z++)
		{
			for (int x = 0; x <= width; x++, i++)
			{
				float tx = ((float)x / (float)m_Size);
				int heightIdx = (z * m_Size) + x;
				float ty = 0f;
				float tz = ((float)z / (float)m_Size);
				m_vertices[i] = new Vector3(tx, ty, tz);
                if (i > 0)
                    t = Vector3.Normalize(m_vertices[i] - m_vertices[i - 1]);
                tangents[i] = new Vector4(t.x, t.y, t.z, -1f);
				uv[i] = new Vector2((float)x / width, (float)z / height);
			}
		}
		m_mesh.vertices = m_vertices;
		m_mesh.uv = uv;

		int[] triangles = new int[width * height * 6];
		for (int ti = 0, vi = 0, y = 0; y < height; y++, vi++)
		{
			for (int x = 0; x < width; x++, ti += 6, vi++)
			{
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + width + 1;
				triangles[ti + 5] = vi + width + 2;
			}
		}
		m_mesh.triangles = triangles;
		m_mesh.RecalculateNormals();
		m_mesh.RecalculateBounds();

		// color material
		MeshRenderer meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = m_OceanMaterial;

		// center terrain
		this.gameObject.transform.localScale = m_Scale;
		this.transform.position = new Vector3(-m_Scale.x * 0.5f, 0f, -m_Scale.z * 0.5f);
	}
}
