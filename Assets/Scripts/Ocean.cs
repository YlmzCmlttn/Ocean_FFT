using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Ocean : MonoBehaviour {
    [Header("Wave Settings")]

    [Range(0.01f, 5.0f)]
    public float amplitude = 1.0f;

    [Range(0.01f, 3.0f)]
    public float frequency = 1.0f;


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
    }

    void UpdateVerticesCPU() {
        if (vertices != null) {
            for (int i = 0; i < vertices.Length; ++i) {
                Vector3 v = transform.TransformPoint(vertices[i]);

                float h = Mathf.Sin(frequency * (v.x + v.z) + Time.time) * amplitude;
                vertices[i].y = h;

                float dx = frequency * amplitude * Mathf.Cos((v.x + v.z) * frequency + Time.time);
                float dy = frequency * amplitude * Mathf.Cos((v.x + v.z) * frequency + Time.time);
                Vector3 n = new Vector3(-dx, 1, -dy);
                n.Normalize();

                normals[i] = n;
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
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
    private void OnDrawGizmos() {
        if (vertices == null) return;

        for (int i = 0; i < vertices.Length; ++i) {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(transform.TransformPoint(vertices[i]), 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.TransformPoint(vertices[i]), normals[i]);
        }
    }
}
