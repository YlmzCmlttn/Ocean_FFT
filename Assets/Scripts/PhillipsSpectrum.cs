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


    public GameObject HTildeDebugQuad;
    public GameObject HTildeSlopeXAxisDebugQuad;
    public GameObject HTildeSlopeZAxisDebugQuad;
    public GameObject HTildeSlopeDisplacementXAxisDebugQuad;
    public GameObject HTildeSlopeDisplacementZAxisDebugQuad;
    public GameObject SwapDebugQuad;
    public GameObject TwiddleFactorDebugQuad;

    private RenderTexture initialSpectrumTexture;
    private RenderTexture updatedSpectrumTexture;
    private RenderTexture heightMapTexture;
    private RenderTexture normalMapTexture;

    private RenderTexture HTildeTexture;
    private RenderTexture HTildeSlopeXAxisTexture;
    private RenderTexture HTildeSlopeZAxisTexture;
    private RenderTexture HTildeSlopeDisplacementXAxisTexture;
    private RenderTexture HTildeSlopeDisplacementZAxisTexture;
    private RenderTexture SwapTexture;
    private RenderTexture TwiddleFactorTexture;



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
    [Range(0.0f, 5.0f)]
    public float _NormalStrength = 1;

    public bool useFFT = true;

    public bool UpdateInitialSpectrum = false;


    private int logN, threadGroupsX, threadGroupsY;

    private int CalculateInitialSpectrumKernelIndex;
    private int UpdateSpectrumForDFTKernelIndex;
    private int HeightMapDFTKernelIndex;
    private int UpdateSpectrumForFFTKernelIndex;
    private int PreComputeTwiddleFactorsAndInputIndicesKernelIndex;
    private int HorizontalStepInverseFFTKernelIndex;
    private int VerticalStepInverseFFTKernelIndex;
    private int PermuteBufferKernelIndex;
    private int AssembleHeightAndNormalMapsKernelIndex;



    private void OnEnable() {

        CreateWaterPlane();
        CreateMaterial();
        threadGroupsX = Mathf.CeilToInt(_Resolution / 8.0f);
        threadGroupsY = Mathf.CeilToInt(_Resolution / 8.0f);

        initialSpectrumTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        initialSpectrumTexture.enableRandomWrite = true;
        initialSpectrumTexture.Create();

        updatedSpectrumTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
        updatedSpectrumTexture.enableRandomWrite = true;
        updatedSpectrumTexture.Create();

        heightMapTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        heightMapTexture.enableRandomWrite = true;
        heightMapTexture.Create();

        normalMapTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        normalMapTexture.enableRandomWrite = true;
        normalMapTexture.Create();

        HTildeTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
        HTildeTexture.enableRandomWrite = true;
        HTildeTexture.Create();
        HTildeSlopeXAxisTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
        HTildeSlopeXAxisTexture.enableRandomWrite = true;
        HTildeSlopeXAxisTexture.Create();
        HTildeSlopeZAxisTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
        HTildeSlopeZAxisTexture.enableRandomWrite = true;
        HTildeSlopeZAxisTexture.Create();
        HTildeSlopeDisplacementXAxisTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
        HTildeSlopeDisplacementXAxisTexture.enableRandomWrite = true;
        HTildeSlopeDisplacementXAxisTexture.Create();
        HTildeSlopeDisplacementZAxisTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
        HTildeSlopeDisplacementZAxisTexture.enableRandomWrite = true;
        HTildeSlopeDisplacementZAxisTexture.Create();

        SwapTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
        SwapTexture.enableRandomWrite = true;
        SwapTexture.Create();


        CalculateInitialSpectrumKernelIndex = fftComputeShader.FindKernel("CS_CalculateInitialSpectrum");
        UpdateSpectrumForDFTKernelIndex = fftComputeShader.FindKernel("CS_UpdateSpectrumForDFT");
        HeightMapDFTKernelIndex = fftComputeShader.FindKernel("CS_HeightMapDFT");
        UpdateSpectrumForFFTKernelIndex = fftComputeShader.FindKernel("CS_UpdateSpectrumForFFT");
        PreComputeTwiddleFactorsAndInputIndicesKernelIndex = fftComputeShader.FindKernel("CS_PreComputeTwiddleFactorsAndInputIndices");
        HorizontalStepInverseFFTKernelIndex = fftComputeShader.FindKernel("CS_HorizontalStepInverseFFT");
        VerticalStepInverseFFTKernelIndex = fftComputeShader.FindKernel("CS_VerticalStepInverseFFT");
        PermuteBufferKernelIndex = fftComputeShader.FindKernel("CS_PermuteBuffer");
        AssembleHeightAndNormalMapsKernelIndex = fftComputeShader.FindKernel("CS_AssembleHeightAndNormalMaps");


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
        fftComputeShader.SetFloat("_NormalStrength", _NormalStrength);


        fftComputeShader.SetTexture(CalculateInitialSpectrumKernelIndex, "_InitialSpectrumTexture", initialSpectrumTexture);
        fftComputeShader.Dispatch(CalculateInitialSpectrumKernelIndex, threadGroupsX, threadGroupsY, 1);

        Material debugMat = initialSpectrumDebugQuad.GetComponent<Renderer>().material;
        debugMat.SetTexture("_DebugTexture", initialSpectrumTexture);

        logN = (int)Mathf.Log(_Resolution, 2);


        TwiddleFactorTexture = new RenderTexture(logN, _Resolution, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        TwiddleFactorTexture.enableRandomWrite = true;
        TwiddleFactorTexture.Create();

        fftComputeShader.SetTexture(PreComputeTwiddleFactorsAndInputIndicesKernelIndex, "_PrecomputeBuffer", TwiddleFactorTexture);
        fftComputeShader.Dispatch(PreComputeTwiddleFactorsAndInputIndicesKernelIndex, logN, (_Resolution / 2) / 8, 1);

        Material debugMat2 = TwiddleFactorDebugQuad.GetComponent<Renderer>().material;
        debugMat2.SetTexture("_DebugTexture", TwiddleFactorTexture);
    }

    void InverseFFT(RenderTexture spectrumTex) {

        bool swap = false;

        fftComputeShader.SetTexture(HorizontalStepInverseFFTKernelIndex, "_PrecomputedData", TwiddleFactorTexture);
        fftComputeShader.SetTexture(HorizontalStepInverseFFTKernelIndex, "_Buffer0", spectrumTex);
        fftComputeShader.SetTexture(HorizontalStepInverseFFTKernelIndex, "_Buffer1", SwapTexture);
        for (int i = 0; i < logN; ++i) {
            swap = !swap;
            fftComputeShader.SetInt("_Step", i);
            fftComputeShader.SetBool("_Swap", swap);
            fftComputeShader.Dispatch(HorizontalStepInverseFFTKernelIndex, threadGroupsX, threadGroupsY, 1);
        }

        fftComputeShader.SetTexture(VerticalStepInverseFFTKernelIndex, "_PrecomputedData", TwiddleFactorTexture);
        fftComputeShader.SetTexture(VerticalStepInverseFFTKernelIndex, "_Buffer0", spectrumTex);
        fftComputeShader.SetTexture(VerticalStepInverseFFTKernelIndex, "_Buffer1", SwapTexture);
        for (int i = 0; i < logN; ++i) {
            swap = !swap;
            fftComputeShader.SetInt("_Step", i);
            fftComputeShader.SetBool("_Swap", swap);
            fftComputeShader.Dispatch(VerticalStepInverseFFTKernelIndex, threadGroupsX, threadGroupsY, 1);
        }

        if (swap) Graphics.Blit(SwapTexture, spectrumTex);

        fftComputeShader.SetTexture(PermuteBufferKernelIndex, "_Buffer0", spectrumTex);
        fftComputeShader.Dispatch(PermuteBufferKernelIndex, threadGroupsX, threadGroupsY, 1);
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
        fftComputeShader.SetFloat("_NormalStrength", _NormalStrength);
        if (!useFFT) {
            fftComputeShader.SetTexture(UpdateSpectrumForDFTKernelIndex, "_InitialSpectrumTexture", initialSpectrumTexture);
            fftComputeShader.SetTexture(UpdateSpectrumForDFTKernelIndex, "_UpdatedSpectrumTexture", updatedSpectrumTexture);
            fftComputeShader.Dispatch(UpdateSpectrumForDFTKernelIndex, threadGroupsX, threadGroupsY, 1);

            Material debugMat = updatedSpectrumDebugQuad.GetComponent<Renderer>().material;
            debugMat.SetTexture("_DebugTexture", updatedSpectrumTexture);

            fftComputeShader.SetTexture(HeightMapDFTKernelIndex, "_UpdatedSpectrumTexture", updatedSpectrumTexture);
            fftComputeShader.SetTexture(HeightMapDFTKernelIndex, "_HeightMap", heightMapTexture);
            fftComputeShader.SetTexture(HeightMapDFTKernelIndex, "_NormalMap", normalMapTexture);
            fftComputeShader.Dispatch(HeightMapDFTKernelIndex, threadGroupsX, threadGroupsY, 1);
        } else {
            fftComputeShader.SetTexture(UpdateSpectrumForFFTKernelIndex, "_InitialSpectrumTexture", initialSpectrumTexture);
            fftComputeShader.SetTexture(UpdateSpectrumForFFTKernelIndex, "_HTildeTexture", HTildeTexture);
            fftComputeShader.SetTexture(UpdateSpectrumForFFTKernelIndex, "_HTildeSlopeXAxisTexture", HTildeSlopeXAxisTexture);
            fftComputeShader.SetTexture(UpdateSpectrumForFFTKernelIndex, "_HTildeSlopeZAxisTexture", HTildeSlopeZAxisTexture);
            fftComputeShader.SetTexture(UpdateSpectrumForFFTKernelIndex, "_HTildeSlopeDisplacementXAxisTexture", HTildeSlopeDisplacementXAxisTexture);
            fftComputeShader.SetTexture(UpdateSpectrumForFFTKernelIndex, "_HTildeSlopeDisplacementZAxisTexture", HTildeSlopeDisplacementZAxisTexture);
            fftComputeShader.Dispatch(UpdateSpectrumForFFTKernelIndex, threadGroupsX, threadGroupsY, 1);



            Material debugMat_1 = HTildeDebugQuad.GetComponent<Renderer>().material;
            debugMat_1.SetTexture("_DebugTexture", HTildeTexture);

            Material debugMat_2 = HTildeSlopeXAxisDebugQuad.GetComponent<Renderer>().material;
            debugMat_2.SetTexture("_DebugTexture", HTildeSlopeXAxisTexture);

            Material debugMat_3 = HTildeSlopeZAxisDebugQuad.GetComponent<Renderer>().material;
            debugMat_3.SetTexture("_DebugTexture", HTildeSlopeZAxisTexture);

            Material debugMat_4 = HTildeSlopeDisplacementXAxisDebugQuad.GetComponent<Renderer>().material;
            debugMat_4.SetTexture("_DebugTexture", HTildeSlopeDisplacementXAxisTexture);

            Material debugMat_5 = HTildeSlopeDisplacementZAxisDebugQuad.GetComponent<Renderer>().material;
            debugMat_5.SetTexture("_DebugTexture", HTildeSlopeDisplacementZAxisTexture);

            InverseFFT(HTildeTexture);
            InverseFFT(HTildeSlopeXAxisTexture);
            InverseFFT(HTildeSlopeZAxisTexture);
            InverseFFT(HTildeSlopeDisplacementXAxisTexture);
            InverseFFT(HTildeSlopeDisplacementZAxisTexture);


            fftComputeShader.SetTexture(AssembleHeightAndNormalMapsKernelIndex, "_HTildeTexture", HTildeTexture);
            fftComputeShader.SetTexture(AssembleHeightAndNormalMapsKernelIndex, "_HTildeSlopeXAxisTexture", HTildeSlopeXAxisTexture);
            fftComputeShader.SetTexture(AssembleHeightAndNormalMapsKernelIndex, "_HTildeSlopeZAxisTexture", HTildeSlopeZAxisTexture);
            fftComputeShader.SetTexture(AssembleHeightAndNormalMapsKernelIndex, "_HTildeSlopeDisplacementXAxisTexture", HTildeSlopeDisplacementXAxisTexture);
            fftComputeShader.SetTexture(AssembleHeightAndNormalMapsKernelIndex, "_HTildeSlopeDisplacementZAxisTexture", HTildeSlopeDisplacementZAxisTexture);
            fftComputeShader.SetTexture(AssembleHeightAndNormalMapsKernelIndex, "_HeightMap", heightMapTexture);
            fftComputeShader.SetTexture(AssembleHeightAndNormalMapsKernelIndex, "_NormalMap", normalMapTexture);
            fftComputeShader.Dispatch(AssembleHeightAndNormalMapsKernelIndex, threadGroupsX, threadGroupsY, 1);
        }

        

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
