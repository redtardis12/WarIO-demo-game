using UnityEngine;

public class EnemyWaveController : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab of the enemy to spawn
    public GameObject player; // Reference to the player's transform
    public Camera mainCamera; // Reference to the main camera

    public float spawnRadius = 10f; // Radius around the player to spawn enemies
    public float minDistanceFromCamera = 5f; // Minimum distance outside the camera view to spawn enemies
    public float initialSpawnDelay = 3f; // Initial delay between spawn attempts
    public float spawnDelayDecreaseRate = 0.1f; // Rate at which spawn delay decreases over time
    public float minSpawnDelay = 0.5f; // Minimum spawn delay
    public int initialEnemiesPerWave = 3; // Initial number of enemies per wave
    public int enemiesPerWaveIncreaseRate = 1; // Rate at which enemies per wave increase over time
    public float waveDuration = 30f; // Total duration of the wave system

    private float timer; // Timer to track wave progression
    private float nextSpawnTime; // Time for the next spawn attempt
    private int currentEnemiesPerWave; // Current number of enemies to spawn per wave

    void Start()
    {
        if (enemyPrefab == null || player == null || mainCamera == null)
        {
            Debug.LogError("Please assign all required fields in the EnemyWaveController script.");
            return;
        }

        timer = 0f;
        nextSpawnTime = 0f;
        currentEnemiesPerWave = initialEnemiesPerWave;
    }

    void Update()
    {
        if (timer >= waveDuration)
        {
            Debug.Log("Wave system completed.");
            enabled = false; // Stop the wave system
            return;
        }

        timer += Time.deltaTime;

        // Gradually decrease spawn delay and increase enemies per wave
        float spawnDelay = Mathf.Max(minSpawnDelay, initialSpawnDelay - (spawnDelayDecreaseRate * timer));
        currentEnemiesPerWave = initialEnemiesPerWave + (int)(enemiesPerWaveIncreaseRate * (timer / waveDuration));

        // Spawn enemies in waves
        if (Time.time >= nextSpawnTime)
        {
            SpawnWave(currentEnemiesPerWave);
            nextSpawnTime = Time.time + spawnDelay;
        }
    }

    void SpawnWave(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        // Calculate a random position outside the camera view but within the spawn radius
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Spawn the enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        enemyAI.enabled = true;
        if (enemyAI != null)
        {
            enemyAI.player = player;
        }
        else
        {
            Debug.LogWarning("Enemy prefab does not have an EnemyAI component.");
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPosition;
        int attempts = 0;
        const int maxAttempts = 100; // Maximum attempts to find a valid spawn position

        do
        {
            // Generate a random direction around the player
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            spawnPosition = player.transform.position + new Vector3(randomDirection.x, 0f, randomDirection.y) * spawnRadius;

            // Ensure the spawn position is outside the camera view
            attempts++;
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Failed to find a valid spawn position after " + maxAttempts + " attempts.");
                break;
            }
        } while (IsPositionVisible(spawnPosition));

        return spawnPosition;
    }

    bool IsPositionVisible(Vector3 position)
    {
        // Convert the world position to viewport space
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);

        // Check if the position is within the camera's viewport bounds
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1 && viewportPoint.z > 0;
    }
}