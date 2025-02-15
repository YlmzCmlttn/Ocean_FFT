using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FFT_Ocean : MonoBehaviour
{
    public Shader waterShader;
    public ComputeShader fftComputeShader;
    public GameObject debugQuad;
    public GameObject debugQuad2;

    public int planeLength = 10;
    public int quadRes = 10;

    private Camera cam;

    private Material waterMaterial;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] normals;

    private RenderTexture heightTex, normalTex;

    public float _Wind_DirX;
    public float _Wind_DirY;
    public float _WindSpeed;
    public float _A;


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
    void CreateMaterial() {
        if (waterShader == null) return;
        if (waterMaterial != null) return;

        waterMaterial = new Material(waterShader);

        MeshRenderer renderer = GetComponent<MeshRenderer>();

        renderer.material = waterMaterial;
    }

    void OnEnable() {
        CreateWaterPlane();
        CreateMaterial();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        heightTex = new RenderTexture(512, 512, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
        heightTex.enableRandomWrite = true;
        heightTex.Create();

        normalTex = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
        normalTex.enableRandomWrite = true;
        normalTex.Create();

        fftComputeShader.SetTexture(0, "_HeightTex", heightTex);
        fftComputeShader.SetTexture(0, "_NormalTex", normalTex);
        fftComputeShader.SetFloat("_FrameTime", Time.time);
        fftComputeShader.SetFloat("_Wind_DirX", _Wind_DirX);
        fftComputeShader.SetFloat("_Wind_DirY", _Wind_DirY);
        fftComputeShader.SetFloat("_WindSpeed", _WindSpeed);
        fftComputeShader.SetFloat("_A", _A);
        fftComputeShader.Dispatch(0, Mathf.CeilToInt(512 / 8.0f), Mathf.CeilToInt(512 / 8.0f), 1);

        waterMaterial.SetTexture("_HeightTex", heightTex);
        waterMaterial.SetTexture("_NormalTex", normalTex);
    }

    void Update()
    {

        fftComputeShader.SetTexture(0, "_HeightTex", heightTex);
        fftComputeShader.SetTexture(0, "_NormalTex", normalTex);
        fftComputeShader.SetFloat("_FrameTime", Time.time);
        fftComputeShader.SetFloat("_Wind_DirX", _Wind_DirX);
        fftComputeShader.SetFloat("_Wind_DirY", _Wind_DirY);
        fftComputeShader.SetFloat("_WindSpeed", _WindSpeed);
        fftComputeShader.SetFloat("_A", _A);
        fftComputeShader.Dispatch(0, Mathf.CeilToInt(512 / 8.0f), Mathf.CeilToInt(512 / 8.0f), 1);
        waterMaterial.SetTexture("_HeightTex", heightTex);
        waterMaterial.SetTexture("_NormalTex", normalTex);

        // Assign height texture to the debug material
        Material debugMat = debugQuad.GetComponent<Renderer>().material;
        debugMat.SetTexture("_DebugTexture", heightTex);
        Material debugMat2 = debugQuad2.GetComponent<Renderer>().material;
        debugMat2.SetTexture("_DebugTexture", normalTex);

    }
    void OnDisable() {
        if (waterMaterial != null) {
            Destroy(waterMaterial);
            waterMaterial = null;
        }

        if (mesh != null) {
            Destroy(mesh);
            mesh = null;
            vertices = null;
            normals = null;
        }
        Destroy(heightTex);
        Destroy(normalTex);
    }
}
