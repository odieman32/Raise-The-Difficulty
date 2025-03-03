using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceWaves : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int waveIndex;
        public int wavePoints;
        public int waveDuration;
        public int maxHitsAllowed;
    }

    public List<Wave> waves = new List<Wave>();
    public int currentWaveIndex = 0;
    public List<EnemySpawn> enemies = new List<EnemySpawn>();
    public List<GameObject> enemiesToSpawn = new List<GameObject>();

    public Transform[] spawnLocation;
    private int spawnIndex = 0;
    private float spawnTimer;
    private bool isSpawning = false;

    public PlayerHit playerHit;
    public List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Start()
    {
        InitializeWaves();
        StartWave();
    }

    private void FixedUpdate()
    {
        spawnedEnemies.RemoveAll(item => item == null);

        if (spawnTimer <= 0 && isSpawning && enemiesToSpawn.Count > 0)
        {
            StartCoroutine(DelaySpawn());
            spawnTimer = 2f;
        }
        else
        {
            spawnTimer -= Time.fixedDeltaTime;
        }

        if (spawnedEnemies.Count == 0 && !isSpawning && enemiesToSpawn.Count == 0)
        {
            StartCoroutine(WaitAndCheckWaveProgress());
        }
    }

    private void InitializeWaves()
    {
        waves.Add(new Wave { waveIndex = 0, wavePoints = 1, waveDuration = 30, maxHitsAllowed = 5 });
        waves.Add(new Wave { waveIndex = 1, wavePoints = 2, waveDuration = 40, maxHitsAllowed = 4 });
        waves.Add(new Wave { waveIndex = 2, wavePoints = 3, waveDuration = 50, maxHitsAllowed = 3 });
    }

    private void StartWave()
    {
        if (currentWaveIndex < waves.Count)
        {
            Debug.Log($"Starting Wave {currentWaveIndex + 1}");
            isSpawning = true;
            playerHit.ResetHits();
            GenerateEnemies(waves[currentWaveIndex].wavePoints);

            if (enemiesToSpawn.Count == 0)
            {
                Debug.LogError($"Wave {currentWaveIndex + 1} Failed - No enemies generated!");
                return;
            }

            Debug.Log($"Wave {currentWaveIndex + 1}: Spawning {enemiesToSpawn.Count} enemies.");

        }
        else
        {
            Debug.Log("All waves completed");
        }
    }

    private void GenerateEnemies(int wavePoints)
    {
        List<GameObject> generatedEnemies = new List<GameObject>();

        int remainingPoints = wavePoints;

        Debug.Log($"Generating Wave {currentWaveIndex + 1}: {wavePoints} points available.");

        while (remainingPoints > 0)
        {
            EnemySpawn selectedEnemy = null;

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i].cost <= remainingPoints)
                {
                    selectedEnemy = enemies[i];
                    break;
                }
            }

            if (selectedEnemy == null)
            {
                Debug.LogWarning($"Wave {currentWaveIndex + 1}: No enemy found for remaining points ({remainingPoints}).");
                break;
            }

            generatedEnemies.Add(selectedEnemy.enemyPrefab);
            remainingPoints -= selectedEnemy.cost;

            Debug.Log($"Wave {currentWaveIndex + 1}: Spawned {selectedEnemy.enemyPrefab.name} (Cost: {selectedEnemy.cost}, Remaining Points: {remainingPoints})");
        }

        if (generatedEnemies.Count == 0)
        {
            Debug.LogError($"Wave {currentWaveIndex + 1} failed! No enemies generated.");
        }

        enemiesToSpawn = generatedEnemies;
        isSpawning = true;
    }

    private IEnumerator DelaySpawn()
    {
        isSpawning = false;
        while (enemiesToSpawn.Count > 0)
        {
            yield return new WaitForSeconds(2f);
            GameObject enemy = Instantiate(enemiesToSpawn[0], spawnLocation[spawnIndex].position, Quaternion.identity);
            spawnedEnemies.Add(enemy);
            enemiesToSpawn.RemoveAt(0);
        }
    }

    private IEnumerator WaitAndCheckWaveProgress()
    {
        yield return new WaitForSeconds(2f);
        CheckWaveProgress();
    }

    private void CheckWaveProgress()
    {
        if (spawnedEnemies.Count > 0)
        {
            Debug.Log("Enemies still exist! Waiting for them to be destroyed.");
            return; // Do NOT progress until all enemies are gone
        }
        if (playerHit.HitCount < waves[currentWaveIndex].maxHitsAllowed)
        {
            if (currentWaveIndex < waves.Count - 1)
            {
                currentWaveIndex++;
                Debug.Log("Wave Complete! Moving to wave " + (currentWaveIndex + 1));
                StartWave();
            }
            else
            {
                Debug.Log("Game Over! All Waves Complete");
            }
        }
        else
        {
            Debug.Log("Too many hits! Restarting Wave" + (currentWaveIndex + 1));
            RestartCurrentWave();
        }
    }

    private void RestartCurrentWave()
    {
        Debug.Log($"Restarting Wave {currentWaveIndex + 1}");
        StartWave();
    }
}


