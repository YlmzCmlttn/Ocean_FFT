using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WaveComputeController : MonoBehaviour {
    public ComputeShader computeShader;
    public float speed = 1f;
    public float magnitude = 0.5f;

    private Mesh _mesh;
    private ComputeBuffer _vertexBuffer;
    private int _kernelHandle;
    private VertexData[] _vertexData;

    struct VertexData {
        public Vector3 position;
    }

    void Start() {
        Initialize();
    }

    void Update() {
        // Calculate offset using sine wave for animation
        float offset = Mathf.Sin(Time.time * speed) * magnitude;

        computeShader.SetFloat("_Offset", offset);
        computeShader.Dispatch(_kernelHandle, Mathf.CeilToInt(_vertexData.Length / 64f), 1, 1);

        // Retrieve modified vertices
        _vertexBuffer.GetData(_vertexData);
        Vector3[] vertices = new Vector3[_vertexData.Length];
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = _vertexData[i].position;

        _mesh.vertices = vertices;
        _mesh.RecalculateNormals();
    }

    void Initialize() {
        _mesh = GetComponent<MeshFilter>().mesh;
        _mesh.MarkDynamic();

        // Create vertex data array
        Vector3[] vertices = _mesh.vertices;
        _vertexData = new VertexData[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            _vertexData[i] = new VertexData { position = vertices[i] };

        // Create compute buffer
        _vertexBuffer = new ComputeBuffer(vertices.Length, 12); // 12 bytes = sizeof(float3)
        _vertexBuffer.SetData(_vertexData);

        // Set up compute shader
        _kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(_kernelHandle, "_VertexBuffer", _vertexBuffer);
    }

    void OnDestroy() {
        if (_vertexBuffer != null)
            _vertexBuffer.Release();
    }
}