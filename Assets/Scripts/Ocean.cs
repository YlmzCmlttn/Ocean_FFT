using System;
using System.Collections;
using System.Collections.Generic;

using static System.Runtime.InteropServices.Marshal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

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



    private Camera cam;
    private Wave[] waves = new Wave[64];


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
    public bool usingPixelShaderNormals = true;
    public bool usingCircularWaves = false;
    public bool randomGeneration = false;
    public bool usingFBM = false;

    public int waveCount = 4;

    public float medianWavelength = 1.0f;
    public float wavelengthRange = 1.0f;
    public float medianDirection = 0.0f;
    public float directionalRange = 30.0f;
    public float medianAmplitude = 1.0f;
    public float medianSpeed = 1.0f;
    public float speedRange = 0.1f;
    public float steepness = 0.0f;

    // FBM Settings
    public int vertexWaveCount = 8;
    public int fragmentWaveCount = 40;

    public float vertexSeed = 0;
    public float vertexSeedIter = 1253.2131f;
    public float vertexFrequency = 1.0f;
    public float vertexFrequencyMult = 1.18f;
    public float vertexAmplitude = 1.0f;
    public float vertexAmplitudeMult = 0.82f;
    public float vertexInitialSpeed = 2.0f;
    public float vertexSpeedRamp = 1.07f;
    public float vertexDrag = 1.0f;
    public float vertexHeight = 1.0f;
    public float vertexMaxPeak = 1.0f;
    public float vertexPeakOffset = 1.0f;
    public float fragmentSeed = 0;
    public float fragmentSeedIter = 1253.2131f;
    public float fragmentFrequency = 1.0f;
    public float fragmentFrequencyMult = 1.18f;
    public float fragmentAmplitude = 1.0f;
    public float fragmentAmplitudeMult = 0.82f;
    public float fragmentInitialSpeed = 2.0f;
    public float fragmentSpeedRamp = 1.07f;
    public float fragmentDrag = 1.0f;
    public float fragmentHeight = 1.0f;
    public float fragmentMaxPeak = 1.0f;
    public float fragmentPeakOffset = 1.0f;

    public float normalStrength = 1;



    [ColorUsageAttribute(false, true)]
    public Color ambient;
    [ColorUsageAttribute(false, true)]
    public Color diffuseReflectance;
    [ColorUsageAttribute(false, true)]
    public Color specularReflectance;
    public float shininess;
    [ColorUsageAttribute(false, true)]
    public Color fresnelColor;

    public float fresnelBias, fresnelStrength, fresnelShininess;
    public float absorptionCoefficient;


    public void ToggleCircularWaves() {
        if (!Application.isPlaying) {
            Debug.Log("Not in play mode!");
            return;
        }

        usingCircularWaves = !usingCircularWaves;

        if (usingCircularWaves) {
            material.EnableKeyword("CIRCULAR_WAVES");
        } else {
            material.DisableKeyword("CIRCULAR_WAVES");
        }
    }

    public void ToggleRandom() {
        if (!Application.isPlaying) {
            Debug.Log("Not in play mode!");
            return;
        }

        randomGeneration = !randomGeneration;
        if (randomGeneration) GenerateNewWaves();
    }
    public void GenerateNewWaves() {
        float wavelengthMin = medianWavelength / (1.0f + wavelengthRange);
        float wavelengthMax = medianWavelength * (1.0f + wavelengthRange);
        float directionMin = medianDirection - directionalRange;
        float directionMax = medianDirection + directionalRange;
        float speedMin = Mathf.Max(0.01f, medianSpeed - speedRange);
        float speedMax = medianSpeed + speedRange;
        float ampOverLen = medianAmplitude / medianWavelength;
        float halfPlaneWidth = planeSize * 0.5f;
        Vector3 minPoint = transform.TransformPoint(new Vector3(-halfPlaneWidth, 0.0f, -halfPlaneWidth));
        Vector3 maxPoint = transform.TransformPoint(new Vector3(halfPlaneWidth, 0.0f, halfPlaneWidth));

        for (int wi = 0; wi < waveCount; ++wi) {
            float wavelength = UnityEngine.Random.Range(wavelengthMin, wavelengthMax);
            float direction = UnityEngine.Random.Range(directionMin, directionMax);
            float amplitude = wavelength * ampOverLen;
            float speed = UnityEngine.Random.Range(speedMin, speedMax);
            Vector2 origin = new Vector2(UnityEngine.Random.Range(minPoint.x * 2, maxPoint.x * 2), UnityEngine.Random.Range(minPoint.x * 2, maxPoint.x * 2));


            waves[wi] = new Wave(wavelength, amplitude, speed, direction, steepness, waveType, origin, waveFunction,waveCount);
        }

        waveBuffer.SetData(waves);
        material.SetBuffer("_Waves", waveBuffer);
    }
    public void ToggleFBM() {
        if (!Application.isPlaying) {
            Debug.Log("Not in play mode!");
            return;
        }

        usingFBM = !usingFBM;

        if (usingFBM) {
            material.EnableKeyword("USE_FBM");
        } else {
            material.DisableKeyword("USE_FBM");
        }
    }

    void CreateWaterPlane() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Water";
        mesh.indexFormat = IndexFormat.UInt32;

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

        if (usingCircularWaves) {
            material.EnableKeyword("CIRCULAR_WAVES");
        } else {
            material.DisableKeyword("CIRCULAR_WAVES");
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
        } else {
            material.DisableKeyword("USE_VERTEX_DISPLACEMENT");
            mesh.vertices = displacedVertices;
            mesh.normals = displacedNormals;
        }
    }

    public void ToggleNormalGeneration() {
        if (!Application.isPlaying) {
            Debug.Log("Not in play mode!");
            return;
        }

        usingPixelShaderNormals = !usingPixelShaderNormals;

        if (usingPixelShaderNormals) {
            material.EnableKeyword("NORMALS_IN_PIXEL_SHADER");
        } else {
            material.DisableKeyword("NORMALS_IN_PIXEL_SHADER");
        }
    }

    private void OnEnable() {
        CreateWaterPlane();
        CreateMaterial();
        CreateWaveBuffer();

        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    void UpdateVerticesCPU() {
        waves[0] = new Wave(wavelength1, amplitude1, speed1, direction1, steepness1, waveType, origin1, waveFunction,waveCount);
        waves[1] = new Wave(wavelength2, amplitude2, speed2, direction2, steepness2, waveType, origin2, waveFunction,waveCount);
        waves[2] = new Wave(wavelength3, amplitude3, speed3, direction3, steepness3, waveType, origin3, waveFunction,waveCount);
        waves[3] = new Wave(wavelength4, amplitude4, speed4, direction4, steepness4, waveType, origin4, waveFunction,waveCount);

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
        waveBuffer = new ComputeBuffer(64, SizeOf(typeof(Wave)));
        material.SetBuffer("_Waves", waveBuffer);
    }

    void Update() {
        material.SetVector("_Ambient", ambient);
        material.SetVector("_DiffuseReflectance", diffuseReflectance);
        material.SetVector("_SpecularReflectance", specularReflectance);
        material.SetVector("_FresnelColor", fresnelColor);
        material.SetFloat("_Shininess", shininess * 100);
        material.SetFloat("_FresnelBias", fresnelBias);
        material.SetFloat("_FresnelStrength", fresnelStrength);
        material.SetFloat("_FresnelShininess", fresnelShininess);
        material.SetFloat("_AbsorptionCoefficient", absorptionCoefficient);
        material.SetInt("_WaveCount", waveCount);

        Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
        Matrix4x4 viewProjMatrix = projMatrix * cam.worldToCameraMatrix;
        material.SetMatrix("_CameraInvViewProjection", viewProjMatrix.inverse);

        if (usingVertexDisplacement) {
            if (updateStatics) {
                if (randomGeneration) {
                    material.SetInt("_VertexWaveCount", vertexWaveCount);
                    material.SetFloat("_VertexSeed", vertexSeed);
                    material.SetFloat("_VertexSeedIter", vertexSeedIter);
                    material.SetFloat("_VertexFrequency", vertexFrequency);
                    material.SetFloat("_VertexFrequencyMult", vertexFrequencyMult);
                    material.SetFloat("_VertexAmplitude", vertexAmplitude);
                    material.SetFloat("_VertexAmplitudeMult", vertexAmplitudeMult);
                    material.SetFloat("_VertexInitialSpeed", vertexInitialSpeed);
                    material.SetFloat("_VertexSpeedRamp", vertexSpeedRamp);
                    material.SetFloat("_VertexDrag", vertexDrag);
                    material.SetFloat("_VertexHeight", vertexHeight);
                    material.SetFloat("_VertexMaxPeak", vertexMaxPeak);
                    material.SetFloat("_VertexPeakOffset", vertexPeakOffset);
                    material.SetInt("_FragmentWaveCount", fragmentWaveCount);
                    material.SetFloat("_FragmentSeed", fragmentSeed);
                    material.SetFloat("_FragmentSeedIter", fragmentSeedIter);
                    material.SetFloat("_FragmentFrequency", fragmentFrequency);
                    material.SetFloat("_FragmentFrequencyMult", fragmentFrequencyMult);
                    material.SetFloat("_FragmentAmplitude", fragmentAmplitude);
                    material.SetFloat("_FragmentAmplitudeMult", fragmentAmplitudeMult);
                    material.SetFloat("_FragmentInitialSpeed", fragmentInitialSpeed);
                    material.SetFloat("_FragmentSpeedRamp", fragmentSpeedRamp);
                    material.SetFloat("_FragmentDrag", fragmentDrag);
                    material.SetFloat("_FragmentHeight", fragmentHeight);
                    material.SetFloat("_FragmentMaxPeak", fragmentMaxPeak);
                    material.SetFloat("_FragmentPeakOffset", fragmentPeakOffset);
                    material.SetFloat("_NormalStrength", normalStrength);

                    material.SetBuffer("_Waves", waveBuffer);
                    return;
                }
                waves[0] = new Wave(wavelength1, amplitude1, speed1, direction1, steepness1, waveType, origin1, waveFunction,waveCount);
                waves[1] = new Wave(wavelength2, amplitude2, speed2, direction2, steepness2, waveType, origin2, waveFunction,waveCount);
                waves[2] = new Wave(wavelength3, amplitude3, speed3, direction3, steepness3, waveType, origin3, waveFunction,waveCount);
                waves[3] = new Wave(wavelength4, amplitude4, speed4, direction4, steepness4, waveType, origin4, waveFunction,waveCount);

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
        /*
        for (int i = 0; i < vertices.Length; ++i) {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(transform.TransformPoint(displacedVertices[i]), 0.1f);
            Gizmos.color = Color.yellow;
            //Gizmos.DrawRay(transform.TransformPoint(displacedVertices[i]), normals[i]);
            Gizmos.DrawRay(transform.TransformPoint(displacedVertices[i]), displacedNormals[i]);
        }
        */
    }
}
