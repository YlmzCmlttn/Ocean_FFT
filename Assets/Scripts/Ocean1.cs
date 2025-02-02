using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Ocean1 : MonoBehaviour {
    [Header("Wave Settings")]
    public int waveCount = 50; // Number of frequency components
    public float significantWaveHeight = 2.0f; // Hs (m)
    public float peakPeriod = 8.0f; // Tp (s)
    public float timeScale = 1.0f; // Speed of time evolution

    private float[] frequencies;
    private float[] amplitudes;
    private float[] phases;
    private Vector2[] waveDirections; // Direction vectors for waves

    private float gravity = 9.81f; // Gravity constant

    public int resolution = 10;
    public int planeSize = 10;
    public Shader shader;

    private Mesh mesh;
    private Material material;
    private Vector3[] vertices;
    private Vector3[] normals;

    void CreateWaterPlane() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Water";

        int vertexCount = resolution * planeSize;
        float halfSize = planeSize * 0.5f;
        vertices = new Vector3[(vertexCount + 1) * (vertexCount + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);

        for (int i = 0, x = 0; x <= vertexCount; ++x) {
            for (int z = 0; z <= vertexCount; ++z, ++i) {
                vertices[i] = new Vector3(((float)x / vertexCount * planeSize) - halfSize, 0, ((float)z / vertexCount * planeSize) - halfSize);
                uvs[i] = new Vector2((float)x / vertexCount, (float)z / vertexCount);
                tangents[i] = tangent;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.tangents = tangents;

        int[] triangles = new int[vertexCount * vertexCount * 6];
        for (int ti = 0, vi = 0, x = 0; x < vertexCount; ++vi, ++x) {
            for (int z = 0; z < vertexCount; ti += 6, ++vi, ++z) {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + 1;
                triangles[ti + 2] = vi + vertexCount + 2;
                triangles[ti + 3] = vi;
                triangles[ti + 4] = vi + vertexCount + 2;
                triangles[ti + 5] = vi + vertexCount + 1;
            }
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        normals = mesh.normals;
    }

    void GenerateWaveComponents() {
        frequencies = new float[waveCount];
        amplitudes = new float[waveCount];
        phases = new float[waveCount];
        waveDirections = new Vector2[waveCount];

        float peakFrequency = 1.0f / peakPeriod;
        float sigmaA = 0.07f, sigmaB = 0.09f;
        float gamma = 3.3f;

        System.Random random = new System.Random();

        for (int i = 0; i < waveCount; i++) {
            // Generate wave frequencies around the peak frequency
            float fMin = peakFrequency / 2f;
            float fMax = peakFrequency * 3f;
            float f = fMin + (float)random.NextDouble() * (fMax - fMin);

            // Compute JONSWAP spectrum
            float sigma = (f <= peakFrequency) ? sigmaA : sigmaB;
            float alpha = 0.0081f;
            float S_f = alpha * gravity * gravity * Mathf.Pow(f, -5) * Mathf.Exp(-5.0f / 4.0f * Mathf.Pow(peakFrequency / f, 4));
            float gammaFactor = Mathf.Exp(-Mathf.Pow(f - peakFrequency, 2) / (2.0f * sigma * sigma * peakFrequency * peakFrequency));
            float Jonswap = S_f * Mathf.Pow(gamma, gammaFactor);

            // Convert spectral density to wave amplitude
            frequencies[i] = f;
            amplitudes[i] = Mathf.Sqrt(2 * Jonswap * (fMax - fMin) / waveCount);
            phases[i] = UnityEngine.Random.Range(0f, 2f * Mathf.PI);

            // Generate a random 2D direction for the wave
            float angle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            waveDirections[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        // Normalize amplitudes based on significant wave height
        float totalEnergy = 0;
        for (int i = 0; i < waveCount; i++)
            totalEnergy += amplitudes[i] * amplitudes[i];

        float A0 = Mathf.Sqrt(2) * significantWaveHeight / Mathf.Sqrt(totalEnergy);
        for (int i = 0; i < waveCount; i++)
            amplitudes[i] *= A0;
    }

    float ComputeWaveHeight(Vector2 position, float time) {
        float height = 0;

        for (int i = 0; i < waveCount; i++) {
            float waveNumber = 2 * Mathf.PI * frequencies[i] / gravity;
            Vector2 direction = waveDirections[i];

            float dotProduct = Vector2.Dot(direction, position);
            height += amplitudes[i] * Mathf.Cos(waveNumber * dotProduct + 2 * Mathf.PI * frequencies[i] * time + phases[i]);
        }

        return height;
    }

    void CreateMaterial() {
        if (shader == null) return;
        if (material != null) return;

        material = new Material(shader);
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = material;
    }

    private void OnEnable() {
        CreateWaterPlane();
        CreateMaterial();
        GenerateWaveComponents();
    }

    void UpdateVerticesCPU() {
        if (vertices != null) {
            for (int i = 0; i < vertices.Length; ++i) {
                Vector3 v = vertices[i];
                Vector2 posXZ = new Vector2(v.x, v.z);

                v.y = ComputeWaveHeight(posXZ, Time.time * timeScale);
                vertices[i] = v;
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
        }
    }

    void Update() {
        UpdateVerticesCPU();
    }

    void OnDisable() {
        if (material != null) {
            Destroy(material);
            material = null;
        }

        if (mesh != null) {
            Destroy(mesh);
            mesh = null;
            vertices = null;
        }
    }
}
