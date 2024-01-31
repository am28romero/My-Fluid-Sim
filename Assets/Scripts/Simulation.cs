using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(ParticleRenderer))]
[RequireComponent(typeof(ParticleSpawner))]
[RequireComponent(typeof(BoxCollider2D))]
public class Simulation : MonoBehaviour
{
    private ParticleSpawner spawner; // Reference to ParticleSpawner class
    private ParticleSpawner.SpawnData spawnData;
    private ParticleRenderer pRenderer;
    private BoxCollider2D boxCollider;
    [SerializeField] private ComputeShader simulationShader; // Reference to compute shader
    [SerializeField] private bool spawnParticles = true; // Radius of particles
    [SerializeField] private uint particleCount = 256; // Number of particles
    [SerializeField] private Vector2[] positions = {}; // Positions of particles
    [SerializeField] private Vector2[] velocities = {}; // Velocities of particles
    [SerializeField] private float gravity = 10.0f; // Gravity strength
    [SerializeField] private float timeStep = 2.0f; // Time step
    [SerializeField] private uint iterations = 1; // Number of iterations per frame
    [SerializeField] private float collisionDamping = 0.9f; // Damping factor for collisions
    private ComputeBuffer positionBuffer; // Buffer for positions
    private ComputeBuffer velocityBuffer; // Buffer for velocities
    private ComputeBuffer collidersBuffer; // Buffer for colliders
    private int kernelIndex; // Index of kernel in compute shader
    private float timer = 0.0f;


    void Start()
    {
        Debug.Log("Simulation Started");
        pRenderer = GetComponent<ParticleRenderer>();
        spawner = GetComponent<ParticleSpawner>();
        boxCollider = GetComponent<BoxCollider2D>();
        Debug.Log(pRenderer.positions.Length);
        
        // Get kernel index and create buffers
        kernelIndex = simulationShader.FindKernel("CSMain");
        positionBuffer = new ComputeBuffer(positions.Length, sizeof(float) * 2);
        velocityBuffer = new ComputeBuffer(velocities.Length, sizeof(float) * 2);

        if (positions.Length == 0 || spawnParticles) {
            spawnData = spawner.GetSpawnData((int) particleCount);
            positions = spawnData.positions;
            velocities = spawnData.velocities;

            // for (int i = velocities.Length; i < positions.Length; i++)
            // {
            //     velocities[i] = new Vector2(0.0f, 0.0f);
            // }
        }
        pRenderer.positions = positions;
        pRenderer.CreateCircle();
    }

    void Update()
    {
        // Update timer
        Time.timeScale = timeStep;
        timer += Time.deltaTime;
        Vector2[] velocitiesBuffer = new Vector2[positions.Length];

        // Get box collider size
        float topCollider = boxCollider.bounds.max.y;
        float bottomCollider = boxCollider.bounds.min.y;
        float leftCollider = boxCollider.bounds.min.x;
        float rightCollider = boxCollider.bounds.max.x;

        // Update positions and velocities in compute shader
        positionBuffer.SetData(positions);
        velocityBuffer.SetData(velocities);
        simulationShader.SetFloat("_DeltaTime", Time.deltaTime);

        simulationShader.SetBuffer(kernelIndex, "_positions", positionBuffer);
        simulationShader.SetBuffer(kernelIndex, "_velocities", velocityBuffer);
        

        for (int i = 0; i < positions.Length; i++)
        {
            // Update velocity
            // velocities[i] *= 0.8f; // Damping factor
            if (i < velocities.Length) velocitiesBuffer[i] = velocities[i];
            velocitiesBuffer[i] += new Vector2(0.0f, -gravity * Time.deltaTime);

            // Update position
            positions[i] += velocitiesBuffer[i] * Time.deltaTime;

            // Check for collision with ground
            if (positions[i].y < bottomCollider)
            {
                positions[i].y = bottomCollider;
                velocitiesBuffer[i].y *= -collisionDamping;
            } else if (positions[i].y > topCollider)
            {
                positions[i].y = topCollider;
                velocitiesBuffer[i].y *= -collisionDamping;
            } else if (positions[i].x < leftCollider)
            {
                positions[i].x = leftCollider;
                velocitiesBuffer[i].x *= -collisionDamping;
            } else if (positions[i].x > rightCollider)
            {
                positions[i].x = rightCollider;
                velocitiesBuffer[i].x *= -collisionDamping;
            }
        }
        velocities = velocitiesBuffer;

        // Update positions in renderer
        pRenderer.positions = positions;
        Debug.Log(pRenderer.positions.Length);

        pRenderer.UpdateCircle();
    }
}
