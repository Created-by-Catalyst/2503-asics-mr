using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleCylinderOrbit : MonoBehaviour
{
    [Header("Cylinder Settings")]
    public float radius = 2f;
    public float length = 3f; // cylinder length along Z axis

    [Header("Movement Settings")]
    public float baseSpeed = 1f;
    public float verticalAmplitude = 0.5f;
    public float verticalFrequency = 1f;
    public bool uniformClockwise = true;

    [Header("Scale (Pulsing) Settings")]
    [Tooltip("How much the particle size changes (0.1 = ±10% of original size)")]
    public float scaleAmount = 0.1f;

    [Tooltip("How fast the particles pulse in size")]
    public float scaleSpeed = 1f;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private float[] angles;
    private float[] speeds;
    private float[] heightOffsets;
    private float[] baseHeights;
    private float[] baseSizes; // store original particle size from Shuriken

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];

        int max = particles.Length;
        angles = new float[max];
        speeds = new float[max];
        heightOffsets = new float[max];
        baseHeights = new float[max];
        baseSizes = new float[max];
    }

    void LateUpdate()
    {
        int aliveCount = ps.GetParticles(particles);

        for (int i = 0; i < aliveCount; i++)
        {
            // Initialize particle-specific values on first update
            if (speeds[i] == 0f)
            {
                angles[i] = Random.Range(0f, Mathf.PI * 2f);
                float randomFactor = Random.Range(0.8f, 1.2f);
                speeds[i] = baseSpeed * randomFactor * (uniformClockwise ? 1f : (Random.value > 0.5f ? 1f : -1f));
                heightOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
                baseHeights[i] = Random.Range(-length * 0.5f, length * 0.5f);
                baseSizes[i] = particles[i].GetCurrentSize(ps); // store Shuriken's size as base
            }

            // Orbit angle update
            angles[i] += speeds[i] * Time.deltaTime;

            // Circular orbit (around Z axis)
            float x = Mathf.Cos(angles[i]) * radius;
            float y = Mathf.Sin(angles[i]) * radius;

            // Gentle float up and down around base height
            float zWave = Mathf.Sin(Time.time * verticalFrequency + heightOffsets[i]) * verticalAmplitude;
            float z = baseHeights[i] + zWave;

            // Apply pulsing scale (gentle breathing effect)
            float scaleWave = 1f + Mathf.Sin(Time.time * scaleSpeed + heightOffsets[i]) * scaleAmount;
            particles[i].startSize = baseSizes[i] * scaleWave;

            particles[i].position = new Vector3(x, y, z);
        }

        ps.SetParticles(particles, aliveCount);
    }
}
