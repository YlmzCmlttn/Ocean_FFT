using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Ocean : MonoBehaviour
{
    public int m_ResolutionOfQuad = 10;

    private Mesh m_Mesh;
    private Vector3[] m_Vertices;

    void CreateWaterPlane()
    {
        GetComponent<MeshFilter>().mesh = m_Mesh = new Mesh();

        m_Vertices = new Vector3[(m_ResolutionOfQuad + 1) * (m_ResolutionOfQuad + 1)];
        for (int i = 0, x = 0; x <= m_ResolutionOfQuad; ++x) {
            for (int z = 0; z <= m_ResolutionOfQuad; ++z, ++i) {
                m_Vertices[i] = new Vector3(x, 0, z);
            }
        }

        m_Mesh.vertices = m_Vertices;
        int[] triangles = new int[m_ResolutionOfQuad * m_ResolutionOfQuad * 6];
        for (int ti = 0, vi = 0, x = 0; x < m_ResolutionOfQuad; ++vi, ++x) {
            for (int z = 0; z < m_ResolutionOfQuad; ti += 6, ++vi, ++z) {
                triangles[ti] = vi;
                triangles[ti + 1] = triangles[ti + 3] = vi + 1;
                triangles[ti + 2] = triangles[ti + 5] = vi + m_ResolutionOfQuad + 1;
                triangles[ti + 4] = vi + m_ResolutionOfQuad + 2;
            }
        }

        m_Mesh.triangles = triangles;

        m_Mesh.RecalculateNormals();

        Vector3[] normals = m_Mesh.normals;

    }

    private void OnEnable() {
        CreateWaterPlane();
    }
    private void OnDrawGizmos() {
        if (m_Vertices == null) return;

        Gizmos.color = Color.white;
        for (int i = 0; i < m_Vertices.Length; ++i)
            Gizmos.DrawSphere(m_Vertices[i], 0.1f);
    }
}
