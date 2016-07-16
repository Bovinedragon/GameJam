using UnityEngine;
using System.Collections;

public class CWaterSimulation : MonoBehaviour {

    public const int c_kernelSize = 13;
    public const int c_width = 128;
    public const int c_height = 128;

    public const float c_alpha = 0.3f;
    public const float c_gravity = 9.81f;

    protected float[] m_kernel;

    // These are all c_width * c_height
    protected float[] m_heights;
    protected float[] m_prevHeights;
    protected float[] m_verticalDerivatives;
    protected float[] m_modifiers;
    protected float[] m_obstructions;

    // Visual representation
    private Mesh m_mesh;
    private Vector3[] m_vertices;

    // Settings
    public Vector3 m_Scale = Vector3.one;
    public Material m_OceanMaterial;
    public ProceduralTexture m_TerrainMask;


    protected uint m_nFrames;

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
            m_obstructions[i] = 1.0f;
        }

        m_nFrames = 0xFFFFFFFF;
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

            m_prevHeights[i] = m_heights[i];
            m_heights[i] = newHeight;
            m_modifiers[i] = 0;
        }
    }


    // Helper
    public void ApplyWave(int _x, int _y, int _radius)
    {
        int iMod = (_y - _radius) * c_width + _x - _radius;
        float RadSq = (float)(_radius * _radius);

        for (int i = -_radius; i <= _radius; i++)
        {
            int iCur = iMod;
            iMod += c_width;
            for (int j = -_radius; j < _radius; j++)
            {
                float Mag = 1.0f - Mathf.Min((float)(i * i + j * j) / RadSq, 1.0f);
                m_modifiers[iCur] += Mag * 0.3f;
                iCur++;
            }
        }
    }


    // Generate height mesh
    private void BuildHeightMask()
    {
        if (m_TerrainMask == null)
            return;

        Color32[] pixels = m_TerrainMask.GetPixels32(0, 0, m_TerrainMask.width, m_TerrainMask.height);

        int iDst = 0;
        for (int y = 0; y < c_height; y++)
        {
            for (int x = 0; x < c_width; x++)
            {
                int iColor = (x * m_TerrainMask.width) / c_width +
                    ((y * m_TerrainMask.height) / c_height) * m_TerrainMask.width;
                m_obstructions[iDst] = (float)pixels[iColor].a < 212 ? 0 : 1.0f;
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
                int heightIdx = (z * c_width) + x;
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

        m_heights = new float[c_width * c_height];
        m_prevHeights = new float[c_width * c_height];
        m_verticalDerivatives = new float[c_width * c_height];
        m_modifiers = new float[c_width * c_height];
        m_obstructions = new float[c_width * c_height];

        BuildOceanMesh();
        BuildHeightMask();
        InitializeKernel();
    }


    // Use this for initialization
    void Start()
    {
        Reset();
	}
	
	// Update is called once per frame
	void Update () {
        PropagateWater(0.01f);

        uint nVerts = c_width * c_height;
        for (uint i = 0; i <  nVerts; i++)
        {
            m_vertices[i].y = m_heights[i];
        }


        // Temp
        if (m_nFrames > 30)
        {
            int xPos = Random.Range(7, 120);
            int yPos = Random.Range(7, 120);
            ApplyWave(xPos, yPos, 7);
            m_nFrames = 0;
        }
        m_nFrames++;

        m_mesh.vertices = m_vertices;
        m_mesh.RecalculateNormals();
    }
}
