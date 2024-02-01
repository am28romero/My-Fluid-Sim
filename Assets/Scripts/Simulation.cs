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
    [SerializeField] private Vector2 gravity = new(0, 9.8f); // Gravity strength
    [SerializeField] private float timeStep = 1.0f; // Time step
    [SerializeField] private uint iterations = 1; // Number of iterations per fram
    [SerializeField] private float collisionDamping = 0.9f; // Damping factor for collisions
    [SerializeField] private uint debugCode = 0b1111; // Debug code
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
        
        if ((DebugCode & 1) == 1) { Debug.Log(pRenderer.positions.Length); }

        if (positions.Length == 0 || spawnParticles) {
            spawnData = spawner.GetSpawnData((int) particleCount);
            positions = spawnData.positions;
            velocities = spawnData.velocities;
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
        Vector2[] velos = new Vector2[positions.Length];

        // Get box collider size
        float topCollider = boxCollider.bounds.max.y;
        float bottomCollider = boxCollider.bounds.min.y;
        float leftCollider = boxCollider.bounds.min.x;
        float rightCollider = boxCollider.bounds.max.x;
        for (int i = 1; i <= iterations; i++)
        {
            // Update positions and velocities in compute shader
            positionBuffer.SetData(Vector2ToFloat2(positions));
            velocityBuffer.SetData(Vector2ToFloat2(velocities));
            simShader.SetFloat("_DeltaTime", Time.deltaTime/i);
            simShader.SetInt("_NumParticles", positions.Length);
            simShader.SetVector("_Gravity", gravity);

            simShader.SetBuffer(kernelIndex, "_positions", positionBuffer);
            simShader.SetBuffer(kernelIndex, "_velocities", velocityBuffer);

            // Update positions and velocities
            simShader.Dispatch(kernelIndex, positions.Length, 1, 1);
            positionBuffer.GetData(positions);
            velocityBuffer.GetData(velocities);
        }
        // for (int i = 0; i < positions.Length; i++)
        // {
        //     // Update velocity
        //     // velocities[i] *= 0.8f; // Damping factor
        //     if (i < velocities.Length) velos[i] = velocities[i];
        //     velos[i] += new Vector2(0.0f, -gravity * Time.deltaTime);

        //     // Update position
        //     positions[i] += velos[i] * Time.deltaTime;

        //     // Check for collision with ground
        //     if (positions[i].y < bottomCollider)
        //     {
        //         positions[i].y = bottomCollider;
        //         velos[i].y *= -collisionDamping;
        //     } else if (positions[i].y > topCollider)
        //     {
        //         positions[i].y = topCollider;
        //         velos[i].y *= -collisionDamping;
        //     } else if (positions[i].x < leftCollider)
        //     {
        //         positions[i].x = leftCollider;
        //         velos[i].x *= -collisionDamping;
        //     } else if (positions[i].x > rightCollider)
        //     {
        //         positions[i].x = rightCollider;
        //         velos[i].x *= -collisionDamping;
        //     }
        // }
        // velocities = velos;

        // Update positions in renderer
        pRenderer.positions = positions;
        if ((DebugCode & 1) == 1) { Debug.Log(pRenderer.positions.Length); }

        pRenderer.UpdateCircle();
    }

    float2[] Vector2ToFloat2(Vector2[] vec2)
    {
        float2[] f2 = new float2[vec2.Length];
        for (int i = 0; i < vec2.Length; i++)
        {
            f2[i] = new float2(vec2[i].x, vec2[i].y);
        }
        return f2;
    }
}
