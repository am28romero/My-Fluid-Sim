using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

public class StandAloneGPURenderer : MonoBehaviour
{
    [SerializeField] private ComputeShader particleComputeShader;

    // Example positions and radius for demonstration
    [SerializeField] private Vector3[] particlePositions;
    [SerializeField] private float radius = 1f;
    [SerializeField] private uint numParticles = 1;
    [SerializeField] private const int SEGMENTS = 8;
    [SerializeField] private float2 xRange = new(-10, 10);
    [SerializeField] private float2 yRange = new(-10, 10);
    // [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private Material material; // Assign your desired material in the Inspector
    [SerializeField] private Mesh mesh;
    [SerializeField] private float updateCircleMeshTimeInMS = 0f; // Mass of the circle
    public Vector3[] computedVertices;
    public int[] computedTriangles;
    private ComputeBuffer positionsBuffer;
    private ComputeBuffer verticesBuffer;
    private ComputeBuffer trianglesBuffer;

    private void Start()
    {
        if (particlePositions.Length == 0)
        {
            particlePositions = RandomPositions(numParticles, xRange, yRange);
        }
        // if (boxCollider2D == null)
        // {
        //     boxCollider2D = GameObject.Find("BoundingBox").GetComponent<BoxCollider2D>();
        //     if (boxCollider2D.gameObject == null)
        //     {
        //         Debug.LogError("BoundingBox not found.");
        //     }
        // }
        // if (mesh == null)
        // {
        //     Debug.Log("Mesh is null. Creating mesh...");
        //     mesh = new Mesh();
        // }
        if (material == null)
        {
            Debug.Log("Material is null. Creating material...");
            material = new Material(Shader.Find("Particles/Additive"));
        }
        material.SetPass(0);
        UpdateCircleMesh();
        Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale));
    }

    private void UpdateCircleMesh()
    {
        Debug.Log("Updating Circle Mesh");

        // pass in the number of segments per circle
        // particleComputeShader.SetInt("segments", (int) SEGMENTS);

        // pass in the radius of the circle
        particleComputeShader.SetFloat("radius", radius);

        // Create and set up the positions buffer
        positionsBuffer = new ComputeBuffer(particlePositions.Length, sizeof(float) * 3);
        positionsBuffer.SetData(particlePositions);

        // Create the buffer to store computed vertices
        int numVertices = particlePositions.Length * (SEGMENTS + 1);
        verticesBuffer = new ComputeBuffer(numVertices, sizeof(float) * 3, ComputeBufferType.Append);

        // Create the buffer to store computed triangles
        int numTriangles = particlePositions.Length * SEGMENTS * 3;
        trianglesBuffer = new ComputeBuffer(numTriangles, sizeof(int) * 3, ComputeBufferType.Append);

        // Set the kernel, buffers, and dispatch the compute shader
        int kernel = particleComputeShader.FindKernel("CSMain");
        particleComputeShader.SetBuffer(kernel, "positionsBuffer", positionsBuffer);
        particleComputeShader.SetBuffer(kernel, "verticesBuffer", verticesBuffer);
        particleComputeShader.SetBuffer(kernel, "trianglesBuffer", trianglesBuffer); 
        // TODO: Remove this ^^ buffer if particles are the same amount of segments
        // TODO: Add functionality for radius
        particleComputeShader.Dispatch(kernel, particlePositions.Length, 1, 1);

        // Read the results from the GPU back to CPU
        computedVertices = new Vector3[numVertices];
        verticesBuffer.GetData(computedVertices);
        computedTriangles = new int[numTriangles];
        trianglesBuffer.GetData(computedTriangles);

        computedVertices = new Vector3[] {
            new Vector3( 0.00f,  0.00f,  0.00f) * radius,
            new Vector3( .707f,  .707f,  0.00f) * radius,
            new Vector3( 0.00f,  1.00f,  0.00f) * radius,
            new Vector3(-.707f,  .707f,  0.00f) * radius,
            new Vector3(-1.00f,  0.00f,  0.00f) * radius,
            new Vector3(-.707f, -.707f,  0.00f) * radius,
            new Vector3( 0.00f, -1.00f,  0.00f) * radius,
            new Vector3( .707f, -.707f,  0.00f) * radius,
            new Vector3( 1.00f,  0.00f,  0.00f) * radius
        };

        computedTriangles = new int[] {
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 6,
            0, 6, 7,
            0, 7, 8,
            0, 8, 1
        };

        // Create or update the mesh using the computed vertices
        mesh.vertices = computedVertices;
        mesh.triangles = computedTriangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Release buffers when done
        positionsBuffer.Release();
        verticesBuffer.Release();
        trianglesBuffer.Release();
    }

    void OnGUI()
    {
        if (material != null)
        {
            // Set the material
            material.SetPass(0);
            float startTime = Time.realtimeSinceStartup;
            UpdateCircleMesh();
            Graphics.DrawMeshNow(mesh, Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale));
            updateCircleMeshTimeInMS = (Time.realtimeSinceStartup - startTime) * 1000;
        }
    }

    // Debug Functions
    private Vector3[] RandomPositions(uint numParticles, float2 xRange, float2 yRange)
    {
        Vector3[] positions = new Vector3[numParticles];
        
        for (int i = 0; i < numParticles; i++)
        {
            // Round to 2 decimal places
            float x = (float) Math.Round(new Random().NextDouble(), 2);
            float y = (float) Math.Round(new Random().NextDouble(), 2);
            positions[i] = new Vector3(x, y, 0);
        }

        return positions;
    }

}