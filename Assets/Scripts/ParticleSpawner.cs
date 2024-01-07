using System.Numerics;
using UnityEditor;
using UnityEngine;
using Unity;
using Vector2 = UnityEngine.Vector2;
using Unity.Mathematics;

public class ParticleSpawner
{
    [SerializeField] private Vector2 initialVelocity = new(0.0f, 0.0f); // Initial speed of particles
    [SerializeField] private Vector2 spawnCenter = new(0.0f, 0.0f); // Centre of the spawn area
    [SerializeField] private float2 spawnSize = new(4.0f, 4.0f); // Size of the spawn area
    [SerializeField] private float jitterStr = 0.0f; // Strength of the jitter

    private SpawnData spawnData;

    // Method to get spawn data for the particle
    public SpawnData GetSpawnData(int particleCount)
    {
        Debug.Log("GetSpawnData() called");
        
        spawnData = new SpawnData(particleCount);
        var rng = new Unity.Mathematics.Random(42);

        float2 s = spawnSize;
        int numX = Mathf.CeilToInt(Mathf.Sqrt(s.x / s.y * particleCount + (s.x - s.y) * (s.x - s.y) / (4 * s.y * s.y)) - (s.x - s.y) / (2 * s.y));
        int numY = Mathf.CeilToInt(particleCount / (float)numX);
        int i = 0;

        for (int y = 0; y < numY; y++)
        {
            for (int x = 0; x < numX; x++)
            {
                if (i >= particleCount) break;

                float tx = numX <= 1 ? 0.5f : x / (numX - 1f);
                float ty = numY <= 1 ? 0.5f : y / (numY - 1f);

                float angle = (float)rng.NextDouble() * 3.14f * 2;
                Vector2 dir = new(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 jitter = ((float)rng.NextDouble() - 0.5f) * jitterStr * dir;
                spawnData.positions[i] = new Vector2((tx - 0.5f) * spawnSize.x, (ty - 0.5f) * spawnSize.y) + jitter + spawnCenter;
                spawnData.velocities[i] = initialVelocity;
                i++;
            }
        }

        return spawnData;
    }

    // Inner class to store spawn data
    public struct SpawnData
    {
        public Vector2[] positions;
        public Vector2[] velocities;

        public SpawnData(int num)
        {
            positions = new Vector2[num];
            velocities = new Vector2[num];
        }
    }
}
