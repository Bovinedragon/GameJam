using UnityEngine;
using System.Collections;

public class CWaterSimulation : MonoBehaviour {

    public const int c_kernelSize = 13;
    public const int c_width = 128;
    public const int c_height = 128;

    public const int c_minObstruction = 104;
    public const int c_maxObstruction = 128;

    public const float c_alpha = 0.3f;
    public const float c_gravity = 9.81f;

    public const float c_waveDecaySeconds = 5.0f;
    public const float c_waveOffsetIntensity = 0.04f;

    protected float[] m_kernel;

    // These are all c_width * c_height
    protected float[] m_heights;
    protected float[] m_prevHeights;
    protected float[] m_verticalDerivatives;
    protected float[] m_modifiers;
    protected float[] m_obstructions;
    protected float[] m_vectorField;

    // Visual representation
    private Mesh m_mesh;
    private Vector3[] m_vertices;

    // Settings
    public Vector3 m_Scale = Vector3.one;
    public Material m_OceanMaterial;
    public Camera m_Camera;
    public BoxCollider m_InputCollider;

    // Input
    protected Vector2 m_prevMouse;
    
	// Other 
	public FishManager m_FishManager;

    // Create wave kernel
    protected void InitializeKernel()
    {
        m_kernel[0] = -0.004262f;
        m_kernel[1] = -0.005641f;
        m_kernel[2] = -0.007456f;
        m_kernel[3] = -0.009704f;
        m_kernel[4] = -0.012145f;
        m_kernel[5] = -0.014159f;
        m_kernel[6] = -0.014957f;
        m_kernel[7] = -0.014159f;
        m_kernel[8] = -0.012145f;
        m_kernel[9] = -0.009704f;
        m_kernel[10] = -0.007456f;
        m_kernel[11] = -0.005641f;
        m_kernel[12] = -0.004262f;
        m_kernel[13] = -0.005641f;
        m_kernel[14] = -0.007999f;
        m_kernel[15] = -0.011578f;
        m_kernel[16] = -0.016790f;
        m_kernel[17] = -0.023277f;
        m_kernel[18] = -0.029073f;
        m_kernel[19] = -0.031433f;
        m_kernel[20] = -0.029073f;
        m_kernel[21] = -0.023277f;
        m_kernel[22] = -0.016790f;
        m_kernel[23] = -0.011578f;
        m_kernel[24] = -0.007999f;
        m_kernel[25] = -0.005641f;
        m_kernel[26] = -0.007456f;
        m_kernel[27] = -0.011578f;
        m_kernel[28] = -0.019007f;
        m_kernel[29] = -0.031433f;
        m_kernel[30] = -0.047263f;
        m_kernel[31] = -0.059965f;
        m_kernel[32] = -0.064448f;
        m_kernel[33] = -0.059965f;
        m_kernel[34] = -0.047263f;
        m_kernel[35] = -0.031433f;
        m_kernel[36] = -0.019007f;
        m_kernel[37] = -0.011578f;
        m_kernel[38] = -0.007456f;
        m_kernel[39] = -0.009704f;
        m_kernel[40] = -0.016790f;
        m_kernel[41] = -0.031433f;
        m_kernel[42] = -0.055537f;
        m_kernel[43] = -0.075938f;
        m_kernel[44] = -0.073775f;
        m_kernel[45] = -0.065328f;
        m_kernel[46] = -0.073775f;
        m_kernel[47] = -0.075938f;
        m_kernel[48] = -0.055537f;
        m_kernel[49] = -0.031433f;
        m_kernel[50] = -0.016790f;
        m_kernel[51] = -0.009704f;
        m_kernel[52] = -0.012145f;
        m_kernel[53] = -0.023277f;
        m_kernel[54] = -0.047263f;
        m_kernel[55] = -0.075938f;
        m_kernel[56] = -0.049939f;
        m_kernel[57] = 0.072258f;
        m_kernel[58] = 0.156421f;
        m_kernel[59] = 0.072258f;
        m_kernel[60] = -0.049939f;
        m_kernel[61] = -0.075938f;
        m_kernel[62] = -0.047263f;
        m_kernel[63] = -0.023277f;
        m_kernel[64] = -0.012145f;
        m_kernel[65] = -0.014159f;
        m_kernel[66] = -0.029073f;
        m_kernel[67] = -0.059965f;
        m_kernel[68] = -0.073775f;
        m_kernel[69] = 0.072258f;
        m_kernel[70] = 0.444565f;
        m_kernel[71] = 0.678277f;
        m_kernel[72] = 0.444565f;
        m_kernel[73] = 0.072258f;
        m_kernel[74] = -0.073775f;
        m_kernel[75] = -0.059965f;
        m_kernel[76] = -0.029073f;
        m_kernel[77] = -0.014159f;
        m_kernel[78] = -0.014957f;
        m_kernel[79] = -0.031433f;
        m_kernel[80] = -0.064448f;
        m_kernel[81] = -0.065328f;
        m_kernel[82] = 0.156421f;
        m_kernel[83] = 0.678277f;
        m_kernel[84] = 1.000000f;
        m_kernel[85] = 0.678277f;
        m_kernel[86] = 0.156421f;
        m_kernel[87] = -0.065328f;
        m_kernel[88] = -0.064448f;
        m_kernel[89] = -0.031433f;
        m_kernel[90] = -0.014957f;
        m_kernel[91] = -0.014159f;
        m_kernel[92] = -0.029073f;
        m_kernel[93] = -0.059965f;
        m_kernel[94] = -0.073775f;
        m_kernel[95] = 0.072258f;
        m_kernel[96] = 0.444565f;
        m_kernel[97] = 0.678277f;
        m_kernel[98] = 0.444565f;
        m_kernel[99] = 0.072258f;
        m_kernel[100] = -0.073775f;
        m_kernel[101] = -0.059965f;
        m_kernel[102] = -0.029073f;
        m_kernel[103] = -0.014159f;
        m_kernel[104] = -0.012145f;
        m_kernel[105] = -0.023277f;
        m_kernel[106] = -0.047263f;
        m_kernel[107] = -0.075938f;
        m_kernel[108] = -0.049939f;
        m_kernel[109] = 0.072258f;
        m_kernel[110] = 0.156421f;
        m_kernel[111] = 0.072258f;
        m_kernel[112] = -0.049939f;
        m_kernel[113] = -0.075938f;
        m_kernel[114] = -0.047263f;
        m_kernel[115] = -0.023277f;
        m_kernel[116] = -0.012145f;
        m_kernel[117] = -0.009704f;
        m_kernel[118] = -0.016790f;
        m_kernel[119] = -0.031433f;
        m_kernel[120] = -0.055537f;
        m_kernel[121] = -0.075938f;
        m_kernel[122] = -0.073775f;
        m_kernel[123] = -0.065328f;
        m_kernel[124] = -0.073775f;
        m_kernel[125] = -0.075938f;
        m_kernel[126] = -0.055537f;
        m_kernel[127] = -0.031433f;
        m_kernel[128] = -0.016790f;
        m_kernel[129] = -0.009704f;
        m_kernel[130] = -0.007456f;
        m_kernel[131] = -0.011578f;
        m_kernel[132] = -0.019007f;
        m_kernel[133] = -0.031433f;
        m_kernel[134] = -0.047263f;
        m_kernel[135] = -0.059965f;
        m_kernel[136] = -0.064448f;
        m_kernel[137] = -0.059965f;
        m_kernel[138] = -0.047263f;
        m_kernel[139] = -0.031433f;
        m_kernel[140] = -0.019007f;
        m_kernel[141] = -0.011578f;
        m_kernel[142] = -0.007456f;
        m_kernel[143] = -0.005641f;
        m_kernel[144] = -0.007999f;
        m_kernel[145] = -0.011578f;
        m_kernel[146] = -0.016790f;
        m_kernel[147] = -0.023277f;
        m_kernel[148] = -0.029073f;
        m_kernel[149] = -0.031433f;
        m_kernel[150] = -0.029073f;
        m_kernel[151] = -0.023277f;
        m_kernel[152] = -0.016790f;
        m_kernel[153] = -0.011578f;
        m_kernel[154] = -0.007999f;
        m_kernel[155] = -0.005641f;
        m_kernel[156] = -0.004262f;
        m_kernel[157] = -0.005641f;
        m_kernel[158] = -0.007456f;
        m_kernel[159] = -0.009704f;
        m_kernel[160] = -0.012145f;
        m_kernel[161] = -0.014159f;
        m_kernel[162] = -0.014957f;
        m_kernel[163] = -0.014159f;
        m_kernel[164] = -0.012145f;
        m_kernel[165] = -0.009704f;
        m_kernel[166] = -0.007456f;
        m_kernel[167] = -0.005641f;
        m_kernel[168] = -0.004262f;
    }


    // Reset all water
    public void Reset()
    {
        for (uint i = 0; i < c_width * c_height; i++)
        {
            m_heights[i] = 0;
            m_prevHeights[i] = 0;
            m_verticalDerivatives[i] = 0;
            m_modifiers[i] = 0;
            m_vectorField[i * 3] = 0;
            m_vectorField[i * 3 + 1] = 0;
            m_vectorField[i * 3 + 2] = 0;
        }
    }


    // Update vertical derivatives
    protected void GetVerticalDerivatives()
    {
        int halfKernel = c_kernelSize >> 1;

        for (int i = 0; i < c_height - c_kernelSize; i++)
        {
            for (int j = 0; j < c_width - c_kernelSize; j++)
            {
                int iNode = (i + halfKernel) * c_width + (j + halfKernel);

                float sum = 0;
                for (int x = 0; x < c_kernelSize; x++)
                {
                    for (int y = 0; y < c_kernelSize; y++)
                    {
                        int iSample = (i + y) * c_width + (j + x);
                        sum += m_kernel[y * c_kernelSize + x] * m_heights[iSample];
                    }
                }
                m_verticalDerivatives[iNode] = sum;
            }
        }
    }


    // Propagate water
    protected void PropagateWater(float _Delta)
    {
        uint nNodes = c_width * c_height;

        // Apply obstruction - set obstructed nodes to 0
        for (uint i = 0; i < nNodes; i++)
            m_heights[i] *= m_obstructions[i];

        GetVerticalDerivatives();

        float alphaTime = c_alpha * _Delta;
        float denominator = 1.0f / (1.0f + alphaTime);
        float gravity = c_gravity * _Delta * _Delta;
        
        for (uint i = 0; i < nNodes; i++)
        {
            float newHeight = m_heights[i] * (2.0f - alphaTime) -
                m_prevHeights[i] -
                gravity * m_verticalDerivatives[i];
            newHeight *= denominator;
            newHeight += m_modifiers[i];
            newHeight *= m_obstructions[i];
            //if (m_obstructions[i] < 1.0f)
            //    newHeight = 10.0f;

            m_prevHeights[i] = m_heights[i];
            m_heights[i] = newHeight;
            m_modifiers[i] = 0;
        }
    }


    // Helper
    public void ApplyWave(int _x0, int _y0, int _x1, int _y1, int _radius, float _Magnitude)
    {
        int xMin, xMax, yMin, yMax;
        if (_x0 < _x1)
        {
            xMin = _x0 - _radius;
            xMax = _x1 + _radius;
        }
        else
        {
            xMin = _x1 - _radius;
            xMax = _x0 + _radius;
        }

        if (_y0 < _y1)
        {
            yMin = _y0 - _radius;
            yMax = _y1 + _radius;
        }
        else
        {
            yMin = _y1 - _radius;
            yMax = _y0 + _radius;
        }

        int iMod = yMin * c_width + xMin;
        int deltaX = _x1 - _x0;
        int deltaY = _y1 - _y0;
        float deltaSqrRcp = 1.0f / (float)(deltaX * deltaX + deltaY * deltaY);

        float radSqr = (float)(_radius * _radius);

        // VF calc
        float dXf = (float)deltaX;
        float dYf = (float)deltaY;
        {
            float sqrRcp = Mathf.Sqrt(deltaSqrRcp);
            dXf *= sqrRcp;
            dYf *= sqrRcp;
        }

        for (int i = yMin; i <= yMax; i++)
        {
            int iCur = iMod;
            iMod += c_width;
            for (int j = xMin; j <= xMax; j++)
            {
                int locX = j - _x0;
                int locY = i - _y0;
                float proj = Mathf.Clamp01((locX * deltaX + locY * deltaY) * deltaSqrRcp);
                float offsX = (deltaX * proj) - locX;
                float offsY = (deltaY * proj) - locY;
                float distSqr = (offsX * offsX + offsY * offsY);
                if (distSqr < radSqr)
                {
                    float Mag = 1.0f - (distSqr / radSqr);
                    m_modifiers[iCur] += Mag * _Magnitude;

                    // Affect vector field - for now, we just assume anything within the wave has full magnitude
                    float prevLen = m_vectorField[i * 3];
                    float totalLenRcp = 1.0f / (prevLen + 1.0f);
                    m_vectorField[iCur * 3 + 1] = (m_vectorField[iCur * 3 + 1] * prevLen + dXf) * totalLenRcp;
                    m_vectorField[iCur * 3 + 2] = (m_vectorField[iCur * 3 + 2] * prevLen + dYf) * totalLenRcp;
                    m_vectorField[iCur * 3] = 1.0f;
                }
                iCur++;
            }
        }
    }


    protected void AdvanceVectorField(float _Delta)
    {
        int nNodes = c_width * c_height * 3;
        for (int i = 0; i < nNodes; i += 3)
        {
            if (m_vectorField[i] <= 0.0f)
                continue;

            m_vectorField[i] = Mathf.Max(m_vectorField[i] - _Delta, 0.0f);
        }
    }


    public Vector2 SampleVectorFieldLocal(int _x, int _y)
    {
        int iCur = ((_y * c_width) + _x) * 3;
        float len = m_vectorField[iCur];
        return new Vector2(m_vectorField[iCur + 1] * len, m_vectorField[iCur + 2] * len);
    }

    public Vector2 SampleVectorFieldWorld(float _x, float _z)
    {
        int locX, locY;
        locX = (int)((_x + m_Scale.x * 0.5f) * c_width / m_Scale.x);
        locY = (int)((_z + m_Scale.z * 0.5f) * c_height / m_Scale.z);

        int iCur = ((locY * c_width) + locX) * 3;
        float len = m_vectorField[iCur];
        return new Vector2(m_vectorField[iCur + 1] * len, m_vectorField[iCur + 2] * len);
    }


    // Generate height mesh
    public void BuildHeightMask(ProceduralTexture texture)
    {
        Color32[] pixels = texture.GetPixels32(0, 0, texture.width, texture.height);

        int iDst = 0;
        for (int y = 0; y < c_height; y++)
        {
            for (int x = 0; x < c_width; x++)
            {
                int iColor = (x * texture.width) / c_width +
                    ((y * texture.height) / c_height) * texture.width;

                float heightFrac = (float)(pixels[iColor].r - c_minObstruction) / (float)(c_maxObstruction - c_minObstruction);
                m_obstructions[iDst] = Mathf.Clamp01(1.0f - heightFrac);

                iDst++;
            }
        }
    }


    // Generate ocean mesh
    private void BuildOceanMesh()
    {
        MeshFilter meshFilter = this.gameObject.AddComponent<MeshFilter>();
        m_mesh = new Mesh();
        meshFilter.mesh = m_mesh;

        int width = c_width - 1;
        int height = c_height - 1;

        // build mesh
        m_vertices = new Vector3[(width + 1) * (height + 1)];
        Vector2[] uv = new Vector2[m_vertices.Length];
        Vector4[] tangents = new Vector4[m_vertices.Length];
        Vector4 t = new Vector4(1f, 0f, 0f, -1f);
        for (int i = 0, z = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                float tx = ((float)x / (float)c_width);
                float ty = 0f;
                float tz = ((float)z / (float)c_width);
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


    // Not quite constructor
    void Awake()
    {
        m_kernel = new float[c_kernelSize * c_kernelSize];
        m_prevMouse = new Vector2(-1, -1);

        uint nVertices = c_width * c_height;
        m_heights = new float[nVertices];
        m_prevHeights = new float[nVertices];
        m_verticalDerivatives = new float[nVertices];
        m_modifiers = new float[nVertices];
        m_obstructions = new float[nVertices];
        m_vectorField = new float[nVertices * 3];

        for (uint i = 0; i < nVertices; i++)
            m_obstructions[i] = 1.0f;

        BuildOceanMesh();
        InitializeKernel();

		if (m_FishManager != null)
			m_FishManager.SetVectorField(m_vectorField, c_width, c_height);
    }
    
    private void HandleInput()
	{
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = m_prevMouse;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (m_InputCollider.Raycast(ray, out hit, 160.0f))
            {
                Vector2 tmp = SampleVectorFieldWorld(hit.point.x, hit.point.z);
                Debug.Log("Vector field at point " + hit.point.x + ", " + hit.point.z + " is " + tmp.x + ", " + tmp.y);
            }
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = m_prevMouse;
            Ray ray = m_Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (m_InputCollider.Raycast(ray, out hit, 160.0f))
            {
                Vector3 pos = hit.point + m_Scale * 0.5f;
                mousePos = new Vector2(pos.x * c_width / m_Scale.x, pos.z * c_height / m_Scale.z);
            }

            int prevX = (int)m_prevMouse.x;
            int prevY = (int)m_prevMouse.y;
            int curX = (int)mousePos.x;
            int curY = (int)mousePos.y;
            int radius = 5;
            int maxX = c_width - radius;
            int maxY = c_height - radius;

            if (prevX != curX || prevY != curY)
            {
                if (prevX > radius && curX > radius && prevY > radius && curY > radius &&
                    prevX <= maxX && curX <= maxX && prevY <= maxY && curY <= maxY &&
                    (prevX != curX || prevY != curY))
                {
                    ApplyWave(prevX, prevY, curX, curY, radius, -0.04f);
                }

                m_prevMouse = mousePos;
            }
        }
        else
        {
            m_prevMouse.x = m_prevMouse.y = -1;
        }
    }

    // Use this for initialization
    void Start()
    {
        Reset();
	}
	
	// Update is called once per frame
	void Update () {
		HandleInput();
        PropagateWater(0.01f);
        AdvanceVectorField(Time.deltaTime / c_waveDecaySeconds);

        uint nVerts = c_width * c_height;
        for (uint i = 0; i <  nVerts; i++)
        {
            m_vertices[i].y = m_heights[i];
        }

        m_mesh.vertices = m_vertices;
        m_mesh.RecalculateNormals();
    }
}
