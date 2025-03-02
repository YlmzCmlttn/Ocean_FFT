using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PhillipsSpectrum : MonoBehaviour
{
    public Shader waterShader;

    public int planeLength = 10;
    public int quadRes = 10;

    private Camera cam;

    private Material waterMaterial;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] normals;


    // Start is called before the first frame update
    public ComputeShader fftComputeShader;
    public GameObject initialSpectrumDebugQuad;
    public GameObject updatedSpectrumDebugQuad;
    public GameObject heightMapDebugQuad;
    public GameObject normalMapDebugQuad;

    private RenderTexture initialSpectrumTexture;
    private RenderTexture updatedSpectrumTexture;
    private RenderTexture heightMapTexture;
    private RenderTexture normalMapTexture;


    public float _Wind_DirX = 1.0f;
    public float _Wind_DirY = 1.0f;
    public float _WindSpeed = 2.0f;
    public float _A = 20;
    [Range(0, 2048)]
    public int _Resolution = 512;
    [Range(0, 2048)]
    public int _PhysicalDomainLength = 512;
    [Range(0.0f, 20.0f)]
    public float _Gravity = 9.8f;
    [Range(0.0f, 200.0f)]
    public float _RepeatTime = 200.0f;
    [Range(0.0f, 100.0f)]
    public float _Damping = 1.0f;
    [Range(0, 100)]
    public int _Seed = 0;
    public Vector2 _MovementLambda = new Vector2(-1.0f,-1.0f);
    public bool UpdateInitialSpectrum = false;


    private int threadGroupsX, threadGroupsY;
    private int CalculateInitialSpectrumKernelIndex;
    private int UpdateSpectrumKernelIndex;
    private int HeightMapKernelIndex;

    private void OnEnable() {

        CreateWaterPlane();
        CreateMaterial();
        threadGroupsX = Mathf.CeilToInt(_Resolution / 8.0f);
        threadGroupsY = Mathf.CeilToInt(_Resolution / 8.0f);

        initialSpectrumTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
        initialSpectrumTexture.enableRandomWrite = true;
        initialSpectrumTexture.Create();

        updatedSpectrumTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
        updatedSpectrumTexture.enableRandomWrite = true;
        updatedSpectrumTexture.Create();

        heightMapTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        heightMapTexture.enableRandomWrite = true;
        heightMapTexture.Create();

        normalMapTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        normalMapTexture.enableRandomWrite = true;
        normalMapTexture.Create();

        

        CalculateInitialSpectrumKernelIndex = fftComputeShader.FindKernel("CS_CalculateInitialSpectrum");
        UpdateSpectrumKernelIndex = fftComputeShader.FindKernel("CS_UpdateSpectrum");
        HeightMapKernelIndex = fftComputeShader.FindKernel("CS_ComputeHeightMap");


        UpdateInitalSpectrumTexture();
    }

    void UpdateInitalSpectrumTexture() {
        fftComputeShader.SetFloat("_Wind_DirX", _Wind_DirX* _WindSpeed);
        fftComputeShader.SetFloat("_Wind_DirY", _Wind_DirY* _WindSpeed);
        fftComputeShader.SetFloat("_WindSpeed", _WindSpeed);
        fftComputeShader.SetFloat("_A", _A / 100000000);
        fftComputeShader.SetInt("_Resolution", _Resolution);
        fftComputeShader.SetInt("_PhysicalDomainLength", _PhysicalDomainLength);
        fftComputeShader.SetFloat("_Gravity", _Gravity);
        fftComputeShader.SetFloat("_RepeatTime", _RepeatTime);
        fftComputeShader.SetFloat("_Damping", _Damping / 100000);
        fftComputeShader.SetInt("_Seed", _Seed);
        fftComputeShader.SetVector("_MovementLambda", _MovementLambda);



        fftComputeShader.SetTexture(CalculateInitialSpectrumKernelIndex, "_InitialSpectrumTexture", initialSpectrumTexture);
        fftComputeShader.Dispatch(CalculateInitialSpectrumKernelIndex, threadGroupsX, threadGroupsY, 1);

        Material debugMat = initialSpectrumDebugQuad.GetComponent<Renderer>().material;
        debugMat.SetTexture("_DebugTexture", initialSpectrumTexture);
    }

    void UpdateSpectrumTexture() {
        fftComputeShader.SetInt("_Resolution", _Resolution);
        fftComputeShader.SetInt("_PhysicalDomainLength", _PhysicalDomainLength);
        fftComputeShader.SetFloat("_FrameTime", Time.time);
        fftComputeShader.SetFloat("_Gravity", _Gravity);
        fftComputeShader.SetFloat("_RepeatTime", _RepeatTime);
        fftComputeShader.SetFloat("_Damping", _Damping / 100000);
        fftComputeShader.SetInt("_Seed", _Seed);
        fftComputeShader.SetVector("_MovementLambda", _MovementLambda);

        fftComputeShader.SetTexture(UpdateSpectrumKernelIndex, "_InitialSpectrumTexture", initialSpectrumTexture);
        fftComputeShader.SetTexture(UpdateSpectrumKernelIndex, "_UpdatedSpectrumTexture", updatedSpectrumTexture);
        fftComputeShader.Dispatch(UpdateSpectrumKernelIndex, threadGroupsX, threadGroupsY, 1);

        fftComputeShader.SetTexture(HeightMapKernelIndex, "_UpdatedSpectrumTexture", updatedSpectrumTexture);
        fftComputeShader.SetTexture(HeightMapKernelIndex, "_HeightMap", heightMapTexture);
        fftComputeShader.SetTexture(HeightMapKernelIndex, "_NormalMap", normalMapTexture);
        fftComputeShader.Dispatch(HeightMapKernelIndex, threadGroupsX, threadGroupsY, 1);

        
        Material debugMat = updatedSpectrumDebugQuad.GetComponent<Renderer>().material;
        debugMat.SetTexture("_DebugTexture", updatedSpectrumTexture);

        Material debugMat2 = heightMapDebugQuad.GetComponent<Renderer>().material;
        debugMat2.SetTexture("_DebugTexture", heightMapTexture);

        Material debugMat3 = normalMapDebugQuad.GetComponent<Renderer>().material;
        debugMat3.SetTexture("_DebugTexture", normalMapTexture);

        waterMaterial.SetTexture("_HeightTex", heightMapTexture);
    }

    // Update is called once per frame
    void Update()
    {
        if (UpdateInitialSpectrum) {
            UpdateInitalSpectrumTexture();
        }
        UpdateSpectrumTexture();
    }
    void CreateMaterial() {
        if (waterShader == null) return;
        if (waterMaterial != null) return;

        waterMaterial = new Material(waterShader);

        MeshRenderer renderer = GetComponent<MeshRenderer>();

        renderer.material = waterMaterial;
    }
    private void CreateWaterPlane() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Water";
        mesh.indexFormat = IndexFormat.UInt32;

        float halfLength = planeLength * 0.5f;
        int sideVertCount = planeLength * quadRes;

        vertices = new Vector3[(sideVertCount + 1) * (sideVertCount + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

        for (int i = 0, x = 0; x <= sideVertCount; ++x) {
            for (int z = 0; z <= sideVertCount; ++z, ++i) {
                vertices[i] = new Vector3(((float)x / sideVertCount * planeLength) - halfLength, 0, ((float)z / sideVertCount * planeLength) - halfLength);
                uv[i] = new Vector2((float)x / sideVertCount, (float)z / sideVertCount);
                tangents[i] = tangent;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.tangents = tangents;

        int[] triangles = new int[sideVertCount * sideVertCount * 6];

        for (int ti = 0, vi = 0, x = 0; x < sideVertCount; ++vi, ++x) {
            for (int z = 0; z < sideVertCount; ti += 6, ++vi, ++z) {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + 1;
                triangles[ti + 2] = vi + sideVertCount + 2;
                triangles[ti + 3] = vi;
                triangles[ti + 4] = vi + sideVertCount + 2;
                triangles[ti + 5] = vi + sideVertCount + 1;
            }
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        normals = mesh.normals;
    }
}
