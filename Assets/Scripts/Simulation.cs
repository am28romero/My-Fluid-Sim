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
    [SerializeField] private ComputeShader simShader; // Reference to compute shader
    [SerializeField] private uint particleCount = 256; // Number of particles
    [SerializeField] private bool spawnParticles = true; // Whether to spawn particles
    [SerializeField] private Vector2[] positions = {}; // Positions of particles
    [SerializeField] private Vector2[] velocities = {}; // Velocities of particles
    [SerializeField] private float gravity = 9.8f; // Gravity strength
    [SerializeField] private float timeStep = 1.0f; // Time step
    [SerializeField] private uint iterations = 1; // Number of iterations per fram
    [SerializeField] private float collisionDamping = 0.9f; // Damping factor for collisions
    [SerializeField] private uint debugCode; // Debug code
    private int kernelIndex; // Index of kernel in compute shader
    private ComputeBuffer positionBuffer; // Buffer for positions
    private ComputeBuffer velocityBuffer; // Buffer for velocities
    private ComputeBuffer colliderBuffer; // Buffer for colliders
    private ComputeBuffer collInstructions; // Buffer for collider instructions
    private float timer = 0.0f;
    public uint DebugCode
    {
        /*
            * Debug code:
            * 0b0001: pRenderer.positions.Length
            * 0b0010: ParticleRenderer Texture2D positions
            * 0b0100: ParticleRenderer SetPositions
            * 0b1000: Unassigned
        */
        get { return debugCode; }
        private set { debugCode = value; }
    }


    void Start()
    {
        Debug.Log("Simulation Started");
        pRenderer = GetComponent<ParticleRenderer>();
        spawner = GetComponent<ParticleSpawner>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        if (DebugCode << 0 == 1) { Debug.Log(pRenderer.positions.Length); }

        if (positions.Length == 0 || spawnParticles) {
            spawnData = spawner.GetSpawnData((int) particleCount);
            positions = spawnData.positions;
            velocities = spawnData.velocities;

            // for (int i = velocities.Length; i < positions.Length; i++)
            // {
            //     velocities[i] = new Vector2(0.0f, 0.0f);
            // }
        }

        // Get kernel index and create buffers
        kernelIndex = simShader.FindKernel("CSMain");
        positionBuffer = new ComputeBuffer(positions.Length, sizeof(float) * 2);
        velocityBuffer = new ComputeBuffer(velocities.Length, sizeof(float) * 2);
        colliderBuffer = new ComputeBuffer(1, sizeof(float) * 4);
        collInstructions = new ComputeBuffer(1, sizeof(float));

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
        simShader.SetFloat("_DeltaTime", Time.deltaTime);

        simShader.SetBuffer(kernelIndex, "_positions", positionBuffer);
        simShader.SetBuffer(kernelIndex, "_velocities", velocityBuffer);

        // Update positions and velocities
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
        if (DebugCode << 0 == 1) { Debug.Log(pRenderer.positions.Length); }

        pRenderer.UpdateCircle();
    }
}
