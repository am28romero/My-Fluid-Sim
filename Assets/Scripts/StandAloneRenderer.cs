using System;
using System.Data.Common;
using TMPro;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StandAloneRenderer : MonoBehaviour
{
    public Mesh originMesh; // Assign your desired mesh in the Inspector
    [SerializeField] private Vector3[] originVertices;
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material; // Assign your desired material in the Inspector
    private const int segments = 8; // Number of segments in the circle
    [Range(0.0f, 4.0f)]
    [SerializeField] private float radius = 0.05f; // Radius of the circle
    public Vector2 position = Vector2.zero; // Position of the circle
    public Vector2 velocity = Vector2.zero; // Velocity of the circle
    [SerializeField] private float updateCircleMeshTimeInMS = 0f; // Mass of the circle
    [SerializeField] private Vector3[] currentVertices;
    [SerializeField] private int[] currentTriangles;
    private float xVectorToRadius;
    private float yVectorToRadius;
    private bool isOnGUICalled = false;


    void Start()
    {
        xVectorToRadius = (float) (1/Math.Cos(2 * Mathf.PI/segments));
        yVectorToRadius = (float) (1/Math.Sin(2 * Mathf.PI/segments));
        Debug.Log("MeshRenderer Started");
        if (originMesh == null)
        {
            Debug.Log("Mesh is null. Creating mesh...");
            originMesh = new Mesh();
        }
        if (material == null)
        {
            Debug.Log("Material is null. Creating material...");
            material = new Material(Shader.Find("Particles/Additive"));
        }
        // Vertices and triangles arrays
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        // Center vertex
        vertices[0] = (Vector3) position; 

        // Generate vertices for the circle
        float angleStep = 2f * Mathf.PI / segments;
        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i;
            vertices[i] = vertices[0] + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

            triangles[(i - 1) * 3] = 0;
            triangles[(i - 1) * 3 + 1] = i;
            triangles[(i - 1) * 3 + 2] = i + 1;
        }

        triangles[^1] = 1;

        originMesh.vertices = vertices;
        originMesh.triangles = triangles;
        originMesh.RecalculateNormals();
        originMesh.RecalculateBounds();

        mesh = originMesh;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        originVertices = vertices;
        currentVertices = mesh.vertices;
        currentTriangles = mesh.triangles;

        Debug.Log("Mesh created");
        Debug.Log($"Vector to radius: {xVectorToRadius} {yVectorToRadius}");
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
            material.SetPass(0);
            float startTime = Time.realtimeSinceStartup;
            UpdateCircleMesh();
            Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale));
            updateCircleMeshTimeInMS = (Time.realtimeSinceStartup - startTime) * 1000;
        }
    }

    void UpdateCircleMesh()
    {
        if (mesh.vertices[0] != (Vector3) position) {
            Debug.Log("Position changed");
            // Vertices and triangles arrays
            Vector3[] vertices = new Vector3[segments + 1];

            // Center vertex
            vertices[0] = (Vector3) position;

            for (int i = 1; i <= segments; i++)
            {
                vertices[i].x = position.x + originVertices[i].x * radius;
                vertices[i].y = position.y + originVertices[i].y * radius;
            }

            mesh.vertices = vertices;
            currentVertices = mesh.vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
        // if the radius is changed
        Vector2 diff = mesh.vertices[1] - new Vector3(radius + position.x, position.y, 0f);
        if (Math.Abs(diff.x) > (mesh.vertices[1].x/100) || Math.Abs(diff.y) > (mesh.vertices[1].y/100))
        {
            Debug.Log($"Radius changed: ({mesh.vertices[1].x}, {mesh.vertices[1].y}) ({radius / xVectorToRadius + position.x}, {radius / yVectorToRadius + position.y})");
            Debug.Log($"Difference: {diff.x}, {diff.y}" + $" ({mesh.vertices[1].x/100}, {mesh.vertices[1].y/100})");
            // Vertices and triangles arrays
            Vector3[] vertices = new Vector3[segments + 1];

            // Center vertex
            vertices[0] = (Vector3)position;

            // Generate vertices for the circle
            for (int i = 1; i <= segments; i++)
            {
                vertices[i] = (Vector3) position + originVertices[i] * radius;
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