using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Ocean : MonoBehaviour
{
    public int m_ResolutionOfQuad = 10;
    public int m_PlaneLength = 10;
    public bool m_DrawGuizmo = false;

    [Range(0.01f, 5.0f)]
    public float m_Amplitude = 1.0f;

    [Range(0.01f, 3.0f)]
    public float m_Frequency = 1.0f;

    public Shader m_Shader;
    private Mesh m_Mesh;
    private Material m_Material;
    private Vector3[] m_Vertices;
    private Vector3[] m_Normals;

    void CreateWaterPlane()
    {
        GetComponent<MeshFilter>().mesh = m_Mesh = new Mesh();
        m_Mesh.name = "Water";
        int  verticesCount = m_ResolutionOfQuad * m_PlaneLength;        
        float halfLength = m_PlaneLength * 0.5f;
        m_Vertices = new Vector3[(verticesCount + 1) * (verticesCount + 1)];
        Vector2[] textureCoordinates = new Vector2[m_Vertices.Length];
        Vector4[] tangents = new Vector4[m_Vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        m_Vertices = new Vector3[(verticesCount + 1) * (verticesCount + 1)];
        for (int i = 0, x = 0; x <= verticesCount; ++x) {
            for (int z = 0; z <= verticesCount; ++z, ++i) {
                m_Vertices[i] = new Vector3(((float)x / verticesCount * m_PlaneLength) - halfLength, 0, ((float)z / verticesCount * m_PlaneLength) - halfLength);                                
                textureCoordinates[i] = new Vector2((float)x / verticesCount, (float)z / verticesCount);
                tangents[i] = tangent;
            }
        }

        m_Mesh.vertices = m_Vertices;
        m_Mesh.uv = textureCoordinates;
        m_Mesh.tangents = tangents;

        m_Mesh.vertices = m_Vertices;
        int[] triangles = new int[verticesCount * verticesCount * 6];
        for (int ti = 0, vi = 0, x = 0; x < verticesCount; ++vi, ++x) {
            for (int z = 0; z < verticesCount; ti += 6, ++vi, ++z) {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + 1;
                triangles[ti + 2] = vi + verticesCount + 2;
                triangles[ti + 3] = vi;
                triangles[ti + 4] = vi + verticesCount + 2;
                triangles[ti + 5] = vi + verticesCount + 1;
            }
        }

        m_Mesh.triangles = triangles;

        m_Mesh.RecalculateNormals();
        m_Normals = m_Mesh.normals;


    }

    void CreateMaterial() {
        if (m_Shader == null) return;
        if (m_Material != null) return;

        m_Material = new Material(m_Shader);
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        renderer.material = m_Material;
    }


    private void OnEnable() {
        CreateWaterPlane();
        CreateMaterial();
    }
    void UpdateVerticesCPU() {
        if (m_Vertices != null) {
            for (int i = 0; i < m_Vertices.Length; ++i) {
                Vector3 v = m_Vertices[i];

                v.y = Mathf.Sin(m_Frequency* (v.x+v.z) + Time.time) * m_Amplitude;
                m_Vertices[i] = v;
            }

            m_Mesh.vertices = m_Vertices;
            m_Mesh.RecalculateNormals();


        }
    }
    void Update() {
        UpdateVerticesCPU();
    }
    void OnDisable() {
        if (m_Material != null) {
            Destroy(m_Material);
            m_Material = null;
        }

        if (m_Mesh != null) {
            Destroy(m_Mesh);
            m_Mesh = null;
            m_Vertices = null;
        }
    }
    private void OnDrawGizmos() {
        if (m_Vertices == null) return;
        if (m_DrawGuizmo) {
            Gizmos.color = Color.white;
            for (int i = 0; i < m_Vertices.Length; ++i)
                Gizmos.DrawSphere(transform.TransformPoint(m_Vertices[i]), 0.1f);
        }
    }
}
