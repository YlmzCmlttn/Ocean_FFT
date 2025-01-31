using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Ocean : MonoBehaviour
{
    public int m_ResolutionOfQuad = 10;
    public Shader m_Shader;
    private Mesh m_Mesh;
    private Material m_Material;
    private Vector3[] m_Vertices;

    void CreateWaterPlane()
    {
        GetComponent<MeshFilter>().mesh = m_Mesh = new Mesh();
        m_Mesh.name = "Water";
        float halfRes = m_ResolutionOfQuad * 0.5f;
        m_Vertices = new Vector3[(m_ResolutionOfQuad + 1) * (m_ResolutionOfQuad + 1)];
        Vector2[] textureCoordinates = new Vector2[m_Vertices.Length];
        Vector4[] tangents = new Vector4[m_Vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        m_Vertices = new Vector3[(m_ResolutionOfQuad + 1) * (m_ResolutionOfQuad + 1)];
        for (int i = 0, x = 0; x <= m_ResolutionOfQuad; ++x) {
            for (int z = 0; z <= m_ResolutionOfQuad; ++z, ++i) {
                m_Vertices[i] = new Vector3(x - halfRes, 0, z - halfRes);
                textureCoordinates[i] = new Vector2((float)x / m_ResolutionOfQuad, (float)z / m_ResolutionOfQuad);
                tangents[i] = tangent;
            }
        }

        m_Mesh.vertices = m_Vertices;
        m_Mesh.uv = textureCoordinates;
        m_Mesh.tangents = tangents;

        m_Mesh.vertices = m_Vertices;
        int[] triangles = new int[m_ResolutionOfQuad * m_ResolutionOfQuad * 6];
        for (int ti = 0, vi = 0, x = 0; x < m_ResolutionOfQuad; ++vi, ++x) {
            for (int z = 0; z < m_ResolutionOfQuad; ti += 6, ++vi, ++z) {
                triangles[ti] = vi;
                triangles[ti + 1] = vi + 1;
                triangles[ti + 2] = vi + m_ResolutionOfQuad + 2;
                triangles[ti + 3] = vi;
                triangles[ti + 4] = vi + m_ResolutionOfQuad + 2;
                triangles[ti + 5] = vi + m_ResolutionOfQuad + 1;
            }
        }

        m_Mesh.triangles = triangles;

        m_Mesh.RecalculateNormals();



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

        Gizmos.color = Color.white;
        for (int i = 0; i < m_Vertices.Length; ++i)
            Gizmos.DrawSphere(transform.TransformPoint(m_Vertices[i]), 0.1f);
    }
}
