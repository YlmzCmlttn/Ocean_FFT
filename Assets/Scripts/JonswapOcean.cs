
using System;
using UnityEngine;

public class JonswapOcean : MonoBehaviour {
    public int gridSize = 64; // Mesh size
    public float waveHeight = 1.5f; // Significant wave height
    public float peakPeriod = 8.0f; // Peak period (T_p)
    public int waveCount = 50; // Number of wave components
    public float timeScale = 1.0f; // Time multiplier for animation
    public float gridSpacing = 1.0f; // Grid spacing

    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] displacedVertices;

    private float[] frequencies;
    private float[] amplitudes;
    private float[] phases;
    private Vector2[] waveDirections;

    private float gravity = 9.81f; // Gravity constant

    void OnEnable() {
        // Create a procedural plane mesh
        CreateMesh();

        // Generate wave parameters using the JONSWAP spectrum
        //GenerateWaveComponents();
    }

    void Update() {
        // Update the wave displacement over time
        //UpdateWaves(Time.time * timeScale);
    }

    void CreateMesh() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        int vertsPerLine = gridSize + 1;
        baseVertices = new Vector3[vertsPerLine * vertsPerLine];
        displacedVertices = new Vector3[baseVertices.Length];
        int[] triangles = new int[gridSize * gridSize * 6];
        Vector2[] uvs = new Vector2[baseVertices.Length];

        // Generate vertices
        for (int z = 0, i = 0; z <= gridSize; z++) {
            for (int x = 0; x <= gridSize; x++, i++) {
                baseVertices[i] = new Vector3(x * gridSpacing, 0, z * gridSpacing);
                uvs[i] = new Vector2((float)x / gridSize, (float)z / gridSize);
            }
        }

        // Generate triangles
        int triIndex = 0;
        for (int z = 0; z < gridSize; z++) {
            for (int x = 0; x < gridSize; x++) {
                int i = z * (gridSize + 1) + x;

                triangles[triIndex++] = i;
                triangles[triIndex++] = i + gridSize + 1;
                triangles[triIndex++] = i + 1;

                triangles[triIndex++] = i + 1;
                triangles[triIndex++] = i + gridSize + 1;
                triangles[triIndex++] = i + gridSize + 2;
            }
        }

        mesh.vertices = baseVertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    void GenerateWaveComponents() {
        frequencies = new float[waveCount];
        amplitudes = new float[waveCount];
        phases = new float[waveCount];
        waveDirections = new Vector2[waveCount];

        float peakFrequency = 1.0f / peakPeriod;
        float sigmaA = 0.07f, sigmaB = 0.09f; // Spread factors
        float gamma = 3.3f; // Peak enhancement factor

        for (int i = 0; i < waveCount; i++) {
            // Random frequency in a range around peak frequency
            float fMin = peakFrequency / 2f;
            float fMax = peakFrequency * 3f;
            float f = fMin + UnityEngine.Random.value * (fMax - fMin);

            float sigma = (f <= peakFrequency) ? sigmaA : sigmaB;
            float alpha = 0.0081f;
            float S_f = alpha * gravity * gravity * Mathf.Pow(f, -5) *
                        Mathf.Exp(-5.0f / 4.0f * Mathf.Pow(peakFrequency / f, 4));

            float gammaFactor = Mathf.Exp(-Mathf.Pow(f - peakFrequency, 2) / (2.0f * sigma * sigma * peakFrequency * peakFrequency));
            float Jonswap = S_f * Mathf.Pow(gamma, gammaFactor);

            frequencies[i] = f;
            amplitudes[i] = Mathf.Sqrt(2 * Jonswap * (fMax - fMin) / waveCount); // Convert to amplitude
            phases[i] = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            waveDirections[i] = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized;
        }

        // Normalize amplitudes based on significant wave height
        float totalEnergy = 0;
        for (int i = 0; i < waveCount; i++) totalEnergy += amplitudes[i] * amplitudes[i];
        float A0 = Mathf.Sqrt(2) * waveHeight / Mathf.Sqrt(totalEnergy);
        for (int i = 0; i < waveCount; i++) amplitudes[i] *= A0;
    }

    void UpdateWaves(float time) {
        displacedVertices = (Vector3[])baseVertices.Clone();

        for (int i = 0; i < displacedVertices.Length; i++) {
            Vector3 vertex = baseVertices[i];
            float waveHeightSum = 0;

            for (int j = 0; j < waveCount; j++) {
                float k = 2 * Mathf.PI * frequencies[j] / gravity;
                Vector2 dir = waveDirections[j];
                float phase = phases[j];

                float waveHeight = amplitudes[j] * Mathf.Cos(2 * Mathf.PI * frequencies[j] * time +
                                                             k * (dir.x * vertex.x + dir.y * vertex.z) + phase);
                waveHeightSum += waveHeight;
            }

            displacedVertices[i].y = waveHeightSum;
        }

        mesh.vertices = displacedVertices;
        mesh.RecalculateNormals();
    }
}
