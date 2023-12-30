using UnityEngine;

public class Simulation : MonoBehaviour
{
    private readonly ParticleSpawner particleSpawner = new(); // Reference to ParticleSpawner class
    [SerializeField] private float spawnInterval = 1.0f; // Interval between spawns
    [SerializeField] private float particleSpeed = 5.0f; // Initial speed of particles
     private float timer = 0.0f;
    private ParticleSpawner.SpawnData spawnData;

    void Start()
    {
        Debug.Log("Simulation Started");
    }

    void Update()
    {
        // Update timer
        timer += Time.deltaTime;

        // Check if it's time to spawn a particle
        if (timer >= spawnInterval)
        {
            // Reset timer
            timer = 0.0f;

            // Get a random position within the screen boundaries
            Vector2 spawnPosition = new(Random.Range(-5f, 5f), Random.Range(-5f, 5f));

            // Spawn a particle using ParticleSpawner
            spawnData = particleSpawner.GetSpawnData(1);

            // Log the spawn position and velocity
            Debug.Log("Particle Spawned - Position: " + spawnData.positions[0] + ", Velocity: " + spawnData.velocities[0]);
        }
    }
}
