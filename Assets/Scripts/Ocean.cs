using System;
using System.Collections;
using System.Collections.Generic;

using static System.Runtime.InteropServices.Marshal;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Ocean : MonoBehaviour {
    public enum WaveFunction {
        Sine = 0,
        SteepSine,
        Gerstner
    };

    public enum WaveType {
        Directional = 0,
        Circular,
    };

    public WaveFunction waveFunction;
    public WaveType waveType;
    public bool updateStatics = true;

    public float direction1 = 0.0f;
    public Vector2 origin1 = new Vector2(0.0f, 0.0f);
    public float speed1 = 1.0f;
    public float amplitude1 = 1.0f;
    public float wavelength1 = 1.0f;
    public float steepness1 = 1.0f;

    public float direction2 = 0.0f;
    public Vector2 origin2 = new Vector2(0.0f, 0.0f);
    public float speed2 = 1.0f;
    public float amplitude2 = 1.0f;
    public float wavelength2 = 1.0f;
    public float steepness2 = 1.0f;

    public float direction3 = 0.0f;
    public Vector2 origin3 = new Vector2(0.0f, 0.0f);
    public float speed3 = 1.0f;
    public float amplitude3 = 1.0f;
    public float wavelength3 = 1.0f;
    public float steepness3 = 1.0f;

    public float direction4 = 0.0f;
    public Vector2 origin4 = new Vector2(0.0f, 0.0f);
    public float speed4 = 1.0f;
    public float amplitude4 = 1.0f;
    public float wavelength4 = 1.0f;
    public float steepness4 = 1.0f;

    private Wave[] waves = new Wave[4];


    public int resolution = 10;
    public int planeSize = 10;
    public Shader shader;
    private ComputeBuffer waveBuffer;

    private Mesh mesh;
    private Material material;
    private Vector3[] vertices;
    private Vector3[] displacedVertices;
    private Vector3[] normals;
    private Vector3[] displacedNormals;

    public bool usingVertexDisplacement = true;

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
        displacedVertices = new Vector3[vertices.Length];
        Array.Copy(vertices, 0, displacedVertices, 0, vertices.Length);
        displacedNormals = new Vector3[normals.Length];
        Array.Copy(normals, 0, displacedNormals, 0, normals.Length);
    }

    public void CycleWaveFunction() {
        if (!Application.isPlaying) {
            Debug.Log("Not in play mode!");
            return;
        }

        switch (waveFunction) {
            case WaveFunction.Sine:
                material.DisableKeyword("SINE_WAVE");
                break;
            case WaveFunction.SteepSine:
                material.DisableKeyword("STEEP_SINE_WAVE");
                break;
            case WaveFunction.Gerstner:
                material.DisableKeyword("GERSTNER_WAVE");
                break;
        }

        waveFunction += 1;
        if ((int)waveFunction > 2) waveFunction = 0;

        switch (waveFunction) {
            case WaveFunction.Sine:
                material.EnableKeyword("SINE_WAVE");
                break;
            case WaveFunction.SteepSine:
                material.EnableKeyword("STEEP_SINE_WAVE");
                break;
            case WaveFunction.Gerstner:
                material.EnableKeyword("GERSTNER_WAVE");
                break;
        }
    }
    void CreateMaterial() {
        if (shader == null) return;
        if (material != null) return;

        material = new Material(shader);

        material.DisableKeyword("USE_VERTEX_DISPLACEMENT");
        material.DisableKeyword("SINE_WAVE");
        material.DisableKeyword("STEEP_SINE_WAVE");
        material.DisableKeyword("GERSTNER_WAVE");
        switch (waveFunction) {
            case WaveFunction.Sine:
                material.EnableKeyword("SINE_WAVE");
                break;
            case WaveFunction.SteepSine:
                material.EnableKeyword("STEEP_SINE_WAVE");
                break;
            case WaveFunction.Gerstner:
                material.EnableKeyword("GERSTNER_WAVE");
                break;
        }

        if (usingVertexDisplacement) {
            material.EnableKeyword("USE_VERTEX_DISPLACEMENT");
            material.SetBuffer("_Waves", waveBuffer);
        } else {
            material.DisableKeyword("USE_VERTEX_DISPLACEMENT");
        }
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = material;
    }

    public void ToggleVertexDisplacementMethod() {
        if (!Application.isPlaying) {
            Debug.Log("Not in play mode!");
            return;
        }

        usingVertexDisplacement = !usingVertexDisplacement;

        if (usingVertexDisplacement) {
            material.EnableKeyword("USE_VERTEX_DISPLACEMENT");
            mesh.vertices = vertices;
            mesh.normals = normals;
            Debug.Log("Toggled GPU Vertex Displacement");
        } else {
            material.DisableKeyword("USE_VERTEX_DISPLACEMENT");
            mesh.vertices = displacedVertices;
            mesh.normals = displacedNormals;
            Debug.Log("Toggled CPU Vertex Displacement");
        }
    }

    private void OnEnable() {
        CreateWaterPlane();
        CreateMaterial();
        CreateWaveBuffer();
    }

    void UpdateVerticesCPU() {
        waves[0] = new Wave(wavelength1, amplitude1, speed1, direction1, steepness1, waveType, origin1, waveFunction);
        waves[1] = new Wave(wavelength2, amplitude2, speed2, direction2, steepness2, waveType, origin2, waveFunction);
        waves[2] = new Wave(wavelength3, amplitude3, speed3, direction3, steepness3, waveType, origin3, waveFunction);
        waves[3] = new Wave(wavelength4, amplitude4, speed4, direction4, steepness4, waveType, origin4, waveFunction);

        if (vertices != null) {
            for (int i = 0; i < vertices.Length; ++i) {
                Vector3 v = transform.TransformPoint(vertices[i]);

                Vector3 newPos = new Vector3(0.0f, 0.0f, 0.0f);
                for (int wi = 0; wi < 4; ++wi) {
                    Wave w = waves[wi];

                    if (waveFunction == WaveFunction.Sine)
                        newPos.y += w.Sine(v);
                    else if (waveFunction == WaveFunction.SteepSine)
                        newPos.y += w.SteepSine(v);
                    else if (waveFunction == WaveFunction.Gerstner) {
                        Vector3 g = w.Gerstner(v);

                        newPos.x += g.x;
                        newPos.z += g.z;
                        newPos.y += g.y;
                    }
                }

                displacedVertices[i] = new Vector3(v.x + newPos.x, newPos.y, v.z + newPos.z);

                // Gerstner waves require the new position to be calculated before normal calculation
                // otherwise could do this in same loop above
                Vector3 normal = new Vector3(0.0f, 0.0f, 0.0f);
                for (int wi = 0; wi < 4; ++wi) {
                    Wave w = waves[wi];

                    if (waveFunction == WaveFunction.Sine) {
                        normal = normal + w.SineNormal(v);
                    } else if (waveFunction == WaveFunction.SteepSine) {
                        normal = normal + w.SteepSineNormal(v);
                    } else if (waveFunction == WaveFunction.Gerstner) {
                        normal = normal + w.GerstnerNormal(displacedVertices[i]);
                    }
                }

                if (waveFunction == WaveFunction.Gerstner) {
                    displacedNormals[i] = new Vector3(-normal.x, 1.0f - normal.y, -normal.z);
                } else {
                    displacedNormals[i] = new Vector3(-normal.x, 1.0f, -normal.y);
                }

                displacedNormals[i].Normalize();
            }

            mesh.vertices = displacedVertices;
            mesh.normals = displacedNormals;
        }
    }

    void CreateWaveBuffer() {
        if (waveBuffer != null) return;
        waveBuffer = new ComputeBuffer(4, SizeOf(typeof(Wave)));
        material.SetBuffer("_Waves", waveBuffer);
    }

        void Update() {
        if (usingVertexDisplacement) {
            if (updateStatics) {
                waves[0] = new Wave(wavelength1, amplitude1, speed1, direction1, steepness1, waveType, origin1, waveFunction);
                waves[1] = new Wave(wavelength2, amplitude2, speed2, direction2, steepness2, waveType, origin2, waveFunction);
                waves[2] = new Wave(wavelength3, amplitude3, speed3, direction3, steepness3, waveType, origin3, waveFunction);
                waves[3] = new Wave(wavelength4, amplitude4, speed4, direction4, steepness4, waveType, origin4, waveFunction);

                waveBuffer.SetData(waves);
                material.SetBuffer("_Waves", waveBuffer);
            }
            
        } else {
            UpdateVerticesCPU();
        }
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
            normals = null;
            displacedVertices = null;
            displacedNormals = null;
            displacedVertices = null;
            displacedNormals = null;
        }

        if (waveBuffer != null) {
            waveBuffer.Release();
            waveBuffer = null;
        }


    }
    private void OnDrawGizmos() {
        if (vertices == null) return;

        for (int i = 0; i < vertices.Length; ++i) {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(transform.TransformPoint(displacedVertices[i]), 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.TransformPoint(displacedVertices[i]), normals[i]);
        }
    }
}
