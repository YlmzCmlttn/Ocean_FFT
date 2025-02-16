using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PhillipsSpectrum : MonoBehaviour
{
    // Start is called before the first frame update
    public ComputeShader fftComputeShader;
    public GameObject debugQuad;
    private RenderTexture initialSpectrumTexture;

    public float _Wind_DirX = -1.0f;
    public float _Wind_DirY = 1.0f;
    public float _WindSpeed = 2.0f;
    public float _A = 20;
    public int _Resolution = 512;
    public int _PhysicalDomainLength = 1024;

    private int threadGroupsX, threadGroupsY;

    private void OnEnable() {
        threadGroupsX = Mathf.CeilToInt(_Resolution / 8.0f);
        threadGroupsY = Mathf.CeilToInt(_Resolution / 8.0f);

        initialSpectrumTexture = new RenderTexture(_Resolution, _Resolution, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
        initialSpectrumTexture.enableRandomWrite = true;
        initialSpectrumTexture.Create();

        fftComputeShader.SetFloat("_Wind_DirX", _Wind_DirX);
        fftComputeShader.SetFloat("_Wind_DirY", _Wind_DirY);
        fftComputeShader.SetFloat("_WindSpeed", _WindSpeed);
        fftComputeShader.SetFloat("_A", _A);
        fftComputeShader.SetInt("_Resolution", _Resolution);
        fftComputeShader.SetInt("_PhysicalDomainLength", _PhysicalDomainLength);
        fftComputeShader.SetTexture(0, "_InitialSpectrumTexture", initialSpectrumTexture);
        fftComputeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Material debugMat = debugQuad.GetComponent<Renderer>().material;
        debugMat.SetTexture("_DebugTexture", initialSpectrumTexture);
    }

    // Update is called once per frame
    void Update()
    {
        fftComputeShader.SetFloat("_Wind_DirX", _Wind_DirX);
        fftComputeShader.SetFloat("_Wind_DirY", _Wind_DirY);
        fftComputeShader.SetFloat("_WindSpeed", _WindSpeed);
        fftComputeShader.SetFloat("_A", _A);
        fftComputeShader.SetInt("_Resolution", _Resolution);
        fftComputeShader.SetInt("_PhysicalDomainLength", _PhysicalDomainLength);
        fftComputeShader.SetTexture(0, "_InitialSpectrumTexture", initialSpectrumTexture);
        fftComputeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Material debugMat = debugQuad.GetComponent<Renderer>().material;
        debugMat.SetTexture("_DebugTexture", initialSpectrumTexture);

    }
}
