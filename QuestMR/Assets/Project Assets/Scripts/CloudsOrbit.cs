using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class CloudOrbit : MonoBehaviour
{
    [Header("Cloud Setup")]
    [SerializeField] private GameObject[] cloudPrefabs;
    [SerializeField] private int cloudsDensity = 20;

    [Header("Positioning")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float radiusOffset = 1f;
    [SerializeField] private float minHeight = -1f;
    [SerializeField] private float maxHeight = 2f;

    [Header("Scaling")]
    [SerializeField] private bool useScaleInEffect = true;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float scaleInDuration = 1.5f;

    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnDelay = 0.05f;
    [SerializeField] private float maxSpawnDelay = 0.25f;

    [Header("Motion Settings")]
    [SerializeField] private float orbitSpeed = 1f;
    [SerializeField, Range(0f, 1f)] private float speedVariation = 0.3f;
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float bobSpeed = 1f;

    // Internal data
    private Transform[] spawnedClouds;
    private float[] baseAngles;
    private float[] speedOffsets;
    private float[] bobOffsets;
    private float[] cloudHeights;
    private float[] cloudRadii;
    private float[] targetScales;

    private bool cloudsInitialized;

    private async void Start()
    {
        if (cloudPrefabs == null || cloudPrefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs assigned to 'Cloud Prefabs'.");
            return;
        }

        if (cloudsDensity <= 0)
        {
            Debug.LogWarning("Cloud density must be greater than zero.");
            return;
        }

        spawnedClouds = new Transform[cloudsDensity];
        baseAngles = new float[cloudsDensity];
        speedOffsets = new float[cloudsDensity];
        bobOffsets = new float[cloudsDensity];
        cloudHeights = new float[cloudsDensity];
        cloudRadii = new float[cloudsDensity];
        targetScales = new float[cloudsDensity];

        // Spawn instantly if scale-in effect is off
        if (!useScaleInEffect)
        {
            for (int i = 0; i < cloudsDensity; i++)
                SpawnCloud(i);
            cloudsInitialized = true;
            return;
        }

        // Spawn first cloud immediately
        SpawnCloud(0);

        // Spawn remaining clouds with staggered random delays
        for (int i = 1; i < cloudsDensity; i++)
        {
            float randomDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            await UniTask.Delay(System.TimeSpan.FromSeconds(randomDelay));
            SpawnCloud(i);
        }

        cloudsInitialized = true;
    }

    private void SpawnCloud(int i)
    {
        GameObject prefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        GameObject cloud = Instantiate(prefab, transform);
        spawnedClouds[i] = cloud.transform;

        targetScales[i] = Random.Range(minScale, maxScale);
        float angle = (360f / cloudsDensity) * i;
        baseAngles[i] = angle;
        speedOffsets[i] = Random.Range(1f - speedVariation, 1f + speedVariation);
        bobOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
        cloudHeights[i] = Random.Range(minHeight, maxHeight);
        cloudRadii[i] = radius + Random.Range(-radiusOffset, radiusOffset);

        Vector3 pos = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad) * cloudRadii[i],
            cloudHeights[i],
            Mathf.Sin(angle * Mathf.Deg2Rad) * cloudRadii[i]
        );
        cloud.transform.localPosition = pos;
        cloud.transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);

        if (useScaleInEffect)
        {
            cloud.transform.localScale = Vector3.zero;
            cloud.transform.DOScale(targetScales[i], scaleInDuration).SetEase(Ease.OutQuad);
        }
        else
        {
            cloud.transform.localScale = Vector3.one * targetScales[i];
        }
    }

    private void OnEnable()
    {
        if (cloudsInitialized && spawnedClouds != null)
        {
            // Start async routine for scaling on re-enable
            ScaleInExistingCloudsAsync().Forget();
        }
    }

    private void OnDisable()
    {
        if (useScaleInEffect)
        {
            foreach (var cloud in spawnedClouds)
            {
                // Reset to zero before scaling up
                cloud.localScale = Vector3.zero;
            }
        }
    }

    private async UniTaskVoid ScaleInExistingCloudsAsync()
    {
        foreach (var cloud in spawnedClouds)
        {
            if (cloud == null) continue;

            cloud.DOKill(); // Prevent overlap

            int index = System.Array.IndexOf(spawnedClouds, cloud);
            float target = (targetScales != null && index >= 0 && index < targetScales.Length)
                ? targetScales[index]
                : 1f;

            if (useScaleInEffect)
            {
                // Add random delay for natural stagger
                float randomDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
                await UniTask.Delay(System.TimeSpan.FromSeconds(randomDelay));

                // Scale back up smoothly
                cloud.DOScale(target, scaleInDuration).SetEase(Ease.OutQuad);
            }
            else
            {
                // Appear instantly
                cloud.localScale = Vector3.one * target;
            }
        }
    }

    private void Update()
    {
        if (spawnedClouds == null) return;

        for (int i = 0; i < spawnedClouds.Length; i++)
        {
            if (spawnedClouds[i] == null) continue;

            baseAngles[i] += Time.deltaTime * orbitSpeed * speedOffsets[i] * 20f;
            if (baseAngles[i] > 360f) baseAngles[i] -= 360f;

            float bob = Mathf.Sin(Time.time * bobSpeed + bobOffsets[i]) * bobAmplitude;

            Vector3 newPos = new Vector3(
                Mathf.Cos(baseAngles[i] * Mathf.Deg2Rad) * cloudRadii[i],
                cloudHeights[i] + bob,
                Mathf.Sin(baseAngles[i] * Mathf.Deg2Rad) * cloudRadii[i]
            );

            spawnedClouds[i].localPosition = newPos;
        }
    }
}
