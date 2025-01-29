#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class MeshGeneratorWindow : EditorWindow
{
    private float width = 10f;
    private float length = 10f;
    private int resX = 100;
    private int resZ = 100;
    private string meshName = "CustomPlane";

    [MenuItem("Tools/Generate High-Res Plane")]
    public static void ShowWindow()
    {
        GetWindow<MeshGeneratorWindow>("Plane Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Plane Settings", EditorStyles.boldLabel);
        width = EditorGUILayout.FloatField("Width", width);
        length = EditorGUILayout.FloatField("Length", length);
        resX = EditorGUILayout.IntField("X Resolution", resX);
        resZ = EditorGUILayout.IntField("Z Resolution", resZ);
        meshName = EditorGUILayout.TextField("Mesh Name", meshName);

        if (GUILayout.Button("Generate and Save Mesh"))
        {
            Mesh mesh = CreatePlaneMesh();
            SaveMeshAsAsset(mesh);
        }
    }

    Mesh CreatePlaneMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = meshName;

        // Vertices
        Vector3[] vertices = new Vector3[(resX + 1) * (resZ + 1)];
        for (int z = 0; z <= resZ; z++)
        {
            float zPos = ((float)z / resZ - 0.5f) * length;
            for (int x = 0; x <= resX; x++)
            {
                float xPos = ((float)x / resX - 0.5f) * width;
                vertices[z * (resX + 1) + x] = new Vector3(xPos, 0, zPos);
            }
        }

        // Triangles
        int[] triangles = new int[resX * resZ * 6];
        int index = 0;
        for (int z = 0; z < resZ; z++)
        {
            for (int x = 0; x < resX; x++)
            {
                int i = z * (resX + 1) + x;
                triangles[index++] = i;
                triangles[index++] = i + resX + 1;
                triangles[index++] = i + 1;
                triangles[index++] = i + 1;
                triangles[index++] = i + resX + 1;
                triangles[index++] = i + resX + 2;
            }
        }

        // UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int z = 0; z <= resZ; z++)
        {
            for (int x = 0; x <= resX; x++)
            {
                uvs[z * (resX + 1) + x] = new Vector2((float)x / resX, (float)z / resZ);
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    void SaveMeshAsAsset(Mesh mesh)
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Mesh Asset",
            meshName + ".asset",
            "asset",
            "Select save location");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = mesh;
        }
    }
}
#endif