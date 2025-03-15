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
        public int maxHitsAllowed;
    }

    #region Lists
    public List<Wave> waves = new List<Wave>();
    public int currentWaveIndex = 0;
    public List<EnemySpawn> enemies = new List<EnemySpawn>();
    public List<GameObject> enemiesToSpawn = new List<GameObject>();
    #endregion

    #region Spawning
    public Transform[] spawnLocation;
    private int spawnIndex = 0;
    private float spawnTimer;
    private bool isSpawning = false;
    #endregion

    #region References
    public PlayerHit playerHit;
    public GameObject upgradePanel;
    public PlayerController playerController;
    private string lastUpgradeType = "";
    public List<GameObject> spawnedEnemies = new List<GameObject>();
    #endregion

    #region Bools/Times
    private bool gameOver = false;
    private bool checkingProgress = false;
    private float waveStartTime = 0f;
    private float minWaveDuration = 5f;
    #endregion
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

        if (!checkingProgress && spawnedEnemies.Count == 0 && !isSpawning && enemiesToSpawn.Count == 0)
        {
            StartCoroutine(WaitAndCheckWaveProgress());
        }
    }

    private void InitializeWaves()
    {
        waves.Clear();
        waves.Add(new Wave { waveIndex = 0, wavePoints = 1, maxHitsAllowed = 8 });
        waves.Add(new Wave { waveIndex = 1, wavePoints = 2, maxHitsAllowed = 7 });
        waves.Add(new Wave { waveIndex = 2, wavePoints = 3, maxHitsAllowed = 6 });
        waves.Add(new Wave { waveIndex = 3, wavePoints = 5, maxHitsAllowed = 5 });
        waves.Add(new Wave { waveIndex = 4, wavePoints = 7, maxHitsAllowed = 4 });
        waves.Add(new Wave { waveIndex = 5, wavePoints = 8, maxHitsAllowed = 3 });
    }

    private void StartWave()
    {
        if (gameOver)
        {
            Debug.Log("Game is Over");
            return;
        }
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("ALl waves complete!");
            gameOver = true;
            return;
        }

        waveStartTime = Time.time;

        Debug.Log($"Starting Wave {currentWaveIndex}");
        isSpawning = true;
        playerHit.ResetHits();
        enemiesToSpawn.Clear(); // Clear previous wave enemies
        spawnedEnemies.Clear(); // Clear any lingering enemies

        GenerateEnemies(waves[currentWaveIndex].wavePoints);

        if (enemiesToSpawn.Count == 0)
        {
            Debug.LogError($"Wave {currentWaveIndex} Failed - No enemies generated!");
            return;
        }

        Debug.Log($"Wave {currentWaveIndex}: Spawning {enemiesToSpawn.Count} enemies.");
    }

    private void GenerateEnemies(int wavePoints)
    {
        List<GameObject> generatedEnemies = new List<GameObject>();

        int remainingPoints = wavePoints;

        Debug.Log($"Generating Wave {currentWaveIndex}: {wavePoints} points available.");

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
            spawnIndex = (spawnIndex + 1) % spawnLocation.Length;
        }
    }

    private IEnumerator WaitAndCheckWaveProgress()
    {
        checkingProgress = true;
        int checkWaveIndex = currentWaveIndex;
        yield return new WaitUntil(() => Time.time - waveStartTime >= minWaveDuration);
        yield return new WaitForSeconds(1f);
        if (currentWaveIndex == checkWaveIndex)
        {
            CheckWaveProgress();
        }
        checkingProgress = false;
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
            ShowUpgradePanel();
        }
        else
        {
            Debug.Log("Down Wave");
            if (!string.IsNullOrEmpty(lastUpgradeType))
            {
                RevertLastUpgrade();
                lastUpgradeType = "";
            }

            if (currentWaveIndex > 0)
            {
                currentWaveIndex--;
            }
            StartWave();
        }
    }

    private void ShowUpgradePanel()
    {
        if (upgradePanel != null)
        {
            Cursor.lockState = CursorLockMode.None;
            upgradePanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Panel not Assigned");
            ApplyUpgrade("");
        }
    }

    public void ApplyUpgrade(string upgradeType)
    {
        lastUpgradeType = upgradeType;
        switch (upgradeType)
        {
            case "Attack":
                playerController.UpgradeAttack();
                break;
            case "Speed":
                playerController.UpgradeSpeed();
                break;
            case "StaminaRecovery":
                playerController.UpgradeStaminaRecovery();
                break;
            default:
                Debug.Log("No upgrade");
                break;
        }

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
        currentWaveIndex++;
        StartWave();
    }

    private void RevertLastUpgrade()
    {
        switch (lastUpgradeType)
        {
            case "Attack":
                playerController.RevertAttack();
                break;
            case "Speed":
                playerController.RevertSpeed();
                break;
            case "StaminaRecovery":
                playerController.RevertStaminaRecovery();
                break;
            default:
                break;
        }
    }
}


