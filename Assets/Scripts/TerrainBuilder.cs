using UnityEngine;
using System.Collections;

public class TerrainBuilder : MonoBehaviour
{
	public ProceduralMaterial m_ProceduralMaterial;
    public Material m_TerrainMaterial;
	public Vector3 m_TerrainSize = new Vector3 (10f, 10f, 10f);
    public CWaterSimulation m_WaterSimulation;
    public int m_RandomSeed = 0;

	private Vector3[] m_vertices;

	private Color32[] m_heightData;
	private int m_heightDataWidth;
	private int m_heightDataHeight;

	void Start()
    {
        if (m_RandomSeed == 0)
            m_RandomSeed = Random.Range(1, 9999);
        StartCoroutine(CreateTerrainMesh());
	}

	public bool HeightDataReady () {
		return m_heightData != null;
	}

	public int SampleHeightDataWorld (float x, float y) {
		if (m_heightData == null)
			return 0;

		int tx = (int)(((x + m_TerrainSize.x / 2) / m_TerrainSize.x) * m_heightDataWidth);
		int ty = (int)(((y + m_TerrainSize.z / 2) / m_TerrainSize.z) * m_heightDataHeight);
		tx = Mathf.Clamp(tx, 0, m_heightDataWidth - 1);
		ty = Mathf.Clamp(ty, 0, m_heightDataHeight - 1);
		return m_heightData[tx + ty * m_heightDataHeight].r;
	}

	private IEnumerator CreateTerrainMesh()
	{
		m_ProceduralMaterial.SetProceduralFloat("$randomseed", m_RandomSeed);
		m_ProceduralMaterial.isReadable = true;
		m_ProceduralMaterial.RebuildTexturesImmediately();

		if (m_ProceduralMaterial.isProcessing)
			yield return null;

		yield return null;

		ProceduralTexture heightTexture = m_ProceduralMaterial.GetGeneratedTexture("TerrainBuilder_height");
		if (heightTexture == null)
		{
			Debug.LogErrorFormat ("Error reading Procedural Height Texture: {0}", m_ProceduralMaterial.name);
			yield break;
		}

		Color32[] heightData = heightTexture.GetPixels32(0, 0, heightTexture.width, heightTexture.height);

		MeshFilter meshFilter = this.gameObject.AddComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		meshFilter.mesh = mesh;

		int width = heightTexture.width-1;
		int height = heightTexture.height-1;

		// build mesh
		m_vertices = new Vector3[(width + 1) * (height + 1)];
		Vector2[] uv = new Vector2[m_vertices.Length];
		Vector4[] tangents = new Vector4[m_vertices.Length];
        Vector4 t = new Vector4(1f, 0f, 0f, -1f);
		for (int i = 0, z = 0; z <= height; z++)
		{
			for (int x = 0; x <= width; x++, i++)
			{
				float tx = ((float)x / (float)heightTexture.width) * m_TerrainSize.x;
				int heightIdx = (z * heightTexture.width) + x;
				float ty = ((float)(heightData[heightIdx].r) / 255.0f) * m_TerrainSize.y;
				float tz = ((float)z / (float)heightTexture.height) * m_TerrainSize.z;
				m_vertices[i] = new Vector3(tx, ty, tz);
                if (i > 0)
                    t = Vector3.Normalize(m_vertices[i] - m_vertices[i - 1]);
                tangents[i] = new Vector4(t.x, t.y, t.z, -1f);
				uv[i] = new Vector2((float)x / width, (float)z / height);
			}
		}
		mesh.vertices = m_vertices;
		mesh.uv = uv;

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
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		// color material
		MeshRenderer meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = m_TerrainMaterial;

		// center terrain
		this.transform.position = new Vector3(-m_TerrainSize.x * 0.5f, -m_TerrainSize.y * 0.5f, -m_TerrainSize.z * 0.5f);

        if (m_WaterSimulation != null)
            m_WaterSimulation.BuildHeightMask(heightTexture);

		m_heightData = heightData;
		m_heightDataWidth = heightTexture.width;
		m_heightDataHeight = heightTexture.height;
	}

	private IEnumerator CreateTerrain()
	{
		m_ProceduralMaterial.SetProceduralFloat("$randomseed", m_RandomSeed);
		m_ProceduralMaterial.isReadable = true;
		m_ProceduralMaterial.RebuildTexturesImmediately();

		if (m_ProceduralMaterial.isProcessing)
			yield return null;

		yield return null;

		ProceduralTexture heightTexture = m_ProceduralMaterial.GetGeneratedTexture("TerrainBuilder_height");
		if (heightTexture == null)
		{
			Debug.LogErrorFormat("Error reading Procedural Height Texture: {0}", m_ProceduralMaterial.name);
			yield break;
		}
		
		TerrainData terrainHeightData = new TerrainData();
		terrainHeightData.heightmapResolution = heightTexture.width;

		terrainHeightData.SetHeights(0, 0, GetHeightsFromTexture(heightTexture));
		//terrainHeightData.SetHeights(0, 0, GetHeightsFromRenderTexture(heightTexture));
		terrainHeightData.size = m_TerrainSize;


		GameObject newTerrainGameObject = Terrain.CreateTerrainGameObject(terrainHeightData);
		newTerrainGameObject.transform.position = new Vector3(-m_TerrainSize.x * 0.5f, -m_TerrainSize.y * 0.5f, -m_TerrainSize.z * 0.5f);
		Terrain terrain = newTerrainGameObject.GetComponent<Terrain>();
		terrain.materialType = Terrain.MaterialType.BuiltInStandard;
		terrain.Flush();
	}

	private float[,] GetHeightsFromTexture(ProceduralTexture proceduralTexture)
	{
		float[,] heightData = new float[proceduralTexture.width, proceduralTexture.height];

		Color32[] textureData = proceduralTexture.GetPixels32(0, 0, proceduralTexture.width, proceduralTexture.height);

		for (int x=0; x < proceduralTexture.width; x++)
		{
			for (int y=0; y < proceduralTexture.height; y++)
			{
				heightData[x, y] = (float)(textureData[(y * proceduralTexture.width) + x].r) / 256.0f;
			}
		}
		return heightData;
	}

	private float[,] GetHeightsFromRenderTexture(ProceduralTexture proceduralTexture)
	{
		float[,] heightData = new float[proceduralTexture.width, proceduralTexture.height];

		// render procedual texture through a temp camera - this will allow for more real-time changes
		Texture2D tempTexture2D = new Texture2D(proceduralTexture.width, proceduralTexture.height, TextureFormat.ARGB32, false, false);
		RenderTexture tempRenderTexture = RenderTexture.GetTemporary(proceduralTexture.width, proceduralTexture.height, 24, RenderTextureFormat.ARGB32);

		GameObject tempCameraGO = new GameObject("RenderTerrainHeightCamera", typeof(Camera));
		Camera tempCamera = tempCameraGO.GetComponent<Camera>();
		tempCamera.enabled = false;
		tempCamera.targetTexture = tempRenderTexture;
		tempCamera.orthographic = true;
		tempCamera.orthographicSize = 0.5f;
		tempCamera.nearClipPlane = -1f;
		tempCamera.farClipPlane = 1f;
		tempCamera.aspect = 1f;

		GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

		Material quadMat = new Material(Shader.Find("Unlit/Texture"));
		quadMat.mainTexture = proceduralTexture;
		Renderer quadRenderer = quad.GetComponent<Renderer>();
		quadRenderer.material = quadMat;

		quad.transform.parent = tempCameraGO.transform;
		quad.transform.localPosition = Vector3.zero;
		tempCamera.transform.position = new Vector3(1000f, 1000f, 1000f);

		tempCamera.Render();
		RenderTexture.active = tempRenderTexture;
		tempTexture2D.ReadPixels(new Rect (0, 0, proceduralTexture.width, proceduralTexture.height), 0, 0);
		tempTexture2D.Apply();
		Color32[] textureData = tempTexture2D.GetPixels32();

		quadMat.mainTexture = tempTexture2D;
		for (int y=0; y < tempTexture2D.height; y++)
		{
			for (int x=0; x < tempTexture2D.width; x++)
			{
				heightData [y, x] = (float)(textureData[(y * x) + x].r) / 256.0f;
			}
		}

		// clean up temp camera render
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary(tempRenderTexture);
		DestroyImmediate(tempCameraGO);
		DestroyImmediate(quadMat);

		return heightData;
	}

}
