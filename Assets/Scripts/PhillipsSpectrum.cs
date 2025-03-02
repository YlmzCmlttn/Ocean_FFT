using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PhillipsSpectrum : MonoBehaviour
{
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


    public float _Wind_DirX = -1.0f;
    public float _Wind_DirY = 1.0f;
    public float _WindSpeed = 2.0f;
    public float _A = 20;
    public int _Resolution = 512;
    public int _PhysicalDomainLength = 1024;
    public bool UpdateInitialSpectrum = false;

    private int threadGroupsX, threadGroupsY;
    private int CalculateInitialSpectrumKernelIndex;
    private int UpdateSpectrumKernelIndex;
    private int HeightMapKernelIndex;

    private void OnEnable() {
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
        fftComputeShader.SetFloat("_Wind_DirX", _Wind_DirX);
        fftComputeShader.SetFloat("_Wind_DirY", _Wind_DirY);
        fftComputeShader.SetFloat("_WindSpeed", _WindSpeed);
        fftComputeShader.SetFloat("_A", _A);
        fftComputeShader.SetInt("_Resolution", _Resolution);
        fftComputeShader.SetInt("_PhysicalDomainLength", _PhysicalDomainLength);
        

        fftComputeShader.SetTexture(CalculateInitialSpectrumKernelIndex, "_InitialSpectrumTexture", initialSpectrumTexture);
        fftComputeShader.Dispatch(CalculateInitialSpectrumKernelIndex, threadGroupsX, threadGroupsY, 1);

        Material debugMat = initialSpectrumDebugQuad.GetComponent<Renderer>().material;
        debugMat.SetTexture("_DebugTexture", initialSpectrumTexture);
    }

    void UpdateSpectrumTexture() {
        fftComputeShader.SetInt("_Resolution", _Resolution);
        fftComputeShader.SetInt("_PhysicalDomainLength", _PhysicalDomainLength);
        fftComputeShader.SetFloat("_FrameTime", Time.time);
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
    }

    // Update is called once per frame
    void Update()
    {
        if (UpdateInitialSpectrum) {
            UpdateInitalSpectrumTexture();
        }
        UpdateSpectrumTexture();
    }
}
