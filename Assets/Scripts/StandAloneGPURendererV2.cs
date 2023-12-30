using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class StandAloneGPURendererV2 : MonoBehaviour
{
    public Mesh originMesh; // Assign your desired mesh in the Inspector
    [SerializeField] private Vector3[] originVertices;
    [SerializeField] private Vector2[] originUVs;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material; // Assign your desired material in the Inspector
    private const int segments = 8; // Number of segments in the circle
    [Range(0.0f, 4.0f)]
    [SerializeField] private float radius = 0.05f; // Radius of the circle
    public Vector3[] position; // Position of the circle
    public Vector2[] velocity; // Velocity of the circle
    [SerializeField] private float updateCircleMeshTimeInMS = 0f; // Mass of the circle
    [SerializeField] private Vector3[] currentVertices;
    [SerializeField] private int[] currentTriangles;
    private bool isOnGUICalled = false;


    void Start()
    {
        Debug.Log("MeshRenderer Started");
        if (originMesh == null)
        {
            Debug.Log("Mesh is null. Creating mesh...");
            originMesh = new Mesh();
        }
        if (material == null)
        {
            Debug.Log("Material is null. Creating material...");
            // Default-Line material
            material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        }
        // Vertices and triangles arrays
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];
        Vector2[] uvs = new Vector2[segments + 1];

        // Calculate vertices and triangles for the circle
        vertices = new Vector3[segments + 1]{
            new( 0.00f,  0.00f,  0.00f),
            new( .707f,  .707f,  0.00f),
            new( 0.00f,  1.00f,  0.00f),
            new(-.707f,  .707f,  0.00f),
            new(-1.00f,  0.00f,  0.00f),
            new(-.707f, -.707f,  0.00f),
            new( 0.00f, -1.00f,  0.00f),
            new( .707f, -.707f,  0.00f),
            new( 1.00f,  0.00f,  0.00f)
        };

        triangles = new int[segments * 3]{
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 6,
            0, 6, 7,
            0, 7, 8,
            0, 8, 1
        };

        uvs[0] = new Vector2(0.5f, 0.5f);
        for (int i = 1; i <= segments; i++)
        {
            uvs[i] = new(0.3f, 0.3f);
        }

        
        originMesh.SetVertices(vertices);
        originMesh.SetTriangles(triangles, 0);
        originMesh.SetUVs(0, uvs);
        // originMesh.RecalculateNormals();
        // originMesh.RecalculateBounds();
        // originMesh.RecalculateTangents();

        mesh = originMesh;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        originVertices = vertices;
        originUVs = uvs;
        currentVertices = mesh.vertices;
        currentTriangles = mesh.triangles;

        Debug.Log("Mesh created");
        // list all vertices
        // for (int i = 0; i < originMesh.vertices.Length; i++)
        // {
        //     Debug.Log("Vertex " + i + ": " + originMesh.vertices[i]);
        // }
        // // list all triangles
        // for (int i = 0; i < originMesh.triangles.Length; i += 3)
        // {
        //     Debug.Log("Triangle " + i / 3 + ": " + originMesh.triangles[i] + ", " + originMesh.triangles[i + 1] + ", " + originMesh.triangles[i + 2]);
        // }
    }

    void OnGUI()
    {
        if (!isOnGUICalled)
        {
            Debug.Log("OnGUI() called");
            isOnGUICalled = true;
        }
        if (material != null && originMesh != null)
        {
            // Set the material
            if (material.SetPass(0))
            {
                // Draw the mesh
                Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale));
            } else
            {
                Debug.LogError("Material.SetPass(0) failed");
                material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            }
            mesh.SetUVs(0, originUVs);
            float startTime = Time.realtimeSinceStartup;
            UpdateCircleMesh();
            updateCircleMeshTimeInMS = (Time.realtimeSinceStartup - startTime) * 1000;
        }
    }

    void UpdateCircleMesh()
    {
        if (mesh.vertices[0] != position[0]) {
            Debug.Log("Position changed");
            // Vertices and triangles arrays
            Vector3[] vertices = new Vector3[segments + 1];

            // Center vertex
            vertices[0] = position[0];

            for (int i = 1; i <= segments; i++)
            {
                vertices[i].x = position[0].x + originVertices[i].x * radius;
                vertices[i].y = position[0].y + originVertices[i].y * radius;
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
        // if the radius is changed
        Vector2 diff = mesh.vertices[1] - new Vector3(radius + position[0].x, position[0].y, 0f);
        if (Math.Abs(diff.x) > (mesh.vertices[1].x/100) || Math.Abs(diff.y) > (mesh.vertices[1].y/100))
        {
            // Vertices and triangles arrays
            Vector3[] vertices = new Vector3[segments + 1];

            // Center vertex
            vertices[0] = position[0];

            // Generate vertices for the circle
            for (int i = 1; i <= segments; i++)
            {
                vertices[i] = position[0] + originVertices[i] * radius;
            }

            // vertices[segments] = (Vector3) position + originVertices[segments] * radius;
            // vertices[segments] = new Vector3(originVertices[segments].x * radius + position.x, originVertices[segments].y * radius + position.y, 0f) + vertices[0];

            mesh.vertices = vertices;
            currentVertices = mesh.vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}