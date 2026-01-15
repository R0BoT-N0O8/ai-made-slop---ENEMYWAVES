using UnityEngine;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnConfig
    {
        public GameObject enemyPrefab;
        [Range(0, 100)] public float spawnWeight = 50f;
    }

    [Header("Wave Settings")]
    [SerializeField] private List<EnemySpawnConfig> enemies;
    [SerializeField] private float timeBetweenSpawns = 2f;
    [SerializeField] private float spawnRadiusMin = 10f;
    [SerializeField] private float spawnRadiusMax = 15f;

    private float nextSpawnTime;
    private Camera mainCamera;
    private Transform player;

    private void Awake()
    {
        mainCamera = Camera.main;
        var playerCtrl = FindFirstObjectByType<PlayerController>();
        if (playerCtrl != null)
        {
            player = playerCtrl.transform;
        }
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime && player != null)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + timeBetweenSpawns;
        }
    }

    private void SpawnEnemy()
    {
        if (enemies == null || enemies.Count == 0) return;

        // Pick random enemy based on weights (simple random for now, or true weighted)
        // Let's do a simple random pick for MVP
        var config = enemies[Random.Range(0, enemies.Count)];
        if (config.enemyPrefab == null) return;

        Vector2 spawnPos = GetValidSpawnPosition();
        if (spawnPos != Vector2.zero)
        {
             Instantiate(config.enemyPrefab, spawnPos, Quaternion.identity);
        }
    }

    private Vector2 GetValidSpawnPosition()
    {
        // Try multiple times to find a valid pos
        for (int i = 0; i < 30; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float distance = Random.Range(spawnRadiusMin, spawnRadiusMax);
            Vector2 tentativePos = (Vector2)player.position + randomDir * distance;

            // Check if outside camera view
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(tentativePos);
            bool onScreen = viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;

            if (!onScreen)
            {
                // Check collision (optional, but good)
                // Assuming enemies are roughly size 1
                if (!Physics2D.OverlapCircle(tentativePos, 0.5f)) 
                {
                    return tentativePos;
                }
            }
        }
        return Vector2.zero; // Failed to find pos
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        // Optionally reset spawn timers or logic here if needed
        nextSpawnTime = Time.time + timeBetweenSpawns; 
    }
}
