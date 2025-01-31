using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HighResPlaneGenerator : MonoBehaviour {
    [Header("Settings")]
    public float width = 10f;     // Plane width
    public float length = 10f;    // Plane length
    public int resX = 100;        // Vertex count along X-axis
    public int resZ = 100;        // Vertex count along Z-axis

    void Start() {
        GeneratePlane();
    }

    void GeneratePlane() {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Create vertices
        Vector3[] vertices = new Vector3[(resX + 1) * (resZ + 1)];
        for (int z = 0; z <= resZ; z++) {
            float zPos = ((float)z / resZ - 0.5f) * length;
            for (int x = 0; x <= resX; x++) {
                float xPos = ((float)x / resX - 0.5f) * width;
                vertices[z * (resX + 1) + x] = new Vector3(xPos, 0, zPos);
            }
        }

        // Create triangles
        int[] triangles = new int[resX * resZ * 6];
        int index = 0;
        for (int z = 0; z < resZ; z++) {
            for (int x = 0; x < resX; x++) {
                int i = z * (resX + 1) + x;
                triangles[index++] = i;
                triangles[index++] = i + resX + 1;
                triangles[index++] = i + 1;
                triangles[index++] = i + 1;
                triangles[index++] = i + resX + 1;
                triangles[index++] = i + resX + 2;
            }
        }

        // Assign data to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.uv = GenerateUVs(resX, resZ);
    }

    Vector2[] GenerateUVs(int resX, int resZ) {
        Vector2[] uvs = new Vector2[(resX + 1) * (resZ + 1)];
        for (int z = 0; z <= resZ; z++) {
            for (int x = 0; x <= resX; x++) {
                uvs[z * (resX + 1) + x] = new Vector2((float)x / resX, (float)z / resZ);
            }
        }
        return uvs;
    }
}