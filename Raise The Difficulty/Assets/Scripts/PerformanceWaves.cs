using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Xml.Serialization;

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
    public UpgradeAnim upgradeAnim;
    public WinAnim winAnim;
    public PlayerController playerController;
    public PauseMenu pauseMenu;
    public Text difficultyIndicator;
    public Text timerText;
    public GameObject winScreen;
    private string lastUpgradeType = "";
    public List<GameObject> spawnedEnemies = new List<GameObject>();
    #endregion

    #region Bools/Times
    private bool gameOver = false;
    private bool checkingProgress = false;
    private float waveStartTime = 0f;
    private float minWaveDuration = 5f;
    public bool GameUpgrade = false;
    private Coroutine progressCoroutine;
    private bool waveInProgress = false;
    private bool upgradeInProgress = false;
    private bool failInProgress = false;
    #endregion

    #region Timer
    public float waveTimeLimit = 30f;
    private float currentWaveTime = 0f;
    #endregion

    private void Start()
    {
        InitializeWaves();
        StartWave();
    }

    private void Update()
    {
        if (!gameOver && !GameUpgrade)
        {
            if (currentWaveIndex >= 5)
            {

                currentWaveTime -= Time.deltaTime;

                if (currentWaveTime < 0)
                {
                    currentWaveTime = 0;
                }
                UpdateTimerUI();

                if (currentWaveTime <= 0 && (spawnedEnemies.Count > 0 || enemiesToSpawn.Count > 0)
                    && !failInProgress && !upgradeInProgress)
                {
                    StartCoroutine(WaveFailure());
                }
            }
            else
            {
                if (timerText != null)
                {
                    timerText.text = "";
                }
            }
        }
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

        if (!checkingProgress && !GameUpgrade && !upgradeInProgress && !waveInProgress && !failInProgress && spawnedEnemies.Count == 0 && !isSpawning && enemiesToSpawn.Count == 0)
        {
            if (progressCoroutine == null)
            {
                progressCoroutine = StartCoroutine(WaitAndCheckWaveProgress());
            }           
        }
    }

    private void InitializeWaves()
    {
        waves.Clear();
        waves.Add(new Wave { waveIndex = 0, wavePoints = 1, maxHitsAllowed = 10 });
        waves.Add(new Wave { waveIndex = 1, wavePoints = 3, maxHitsAllowed = 10 });
        waves.Add(new Wave { waveIndex = 2, wavePoints = 4, maxHitsAllowed = 10 });
        waves.Add(new Wave { waveIndex = 3, wavePoints = 7, maxHitsAllowed = 9 });
        waves.Add(new Wave { waveIndex = 4, wavePoints = 9, maxHitsAllowed = 9 });
        waves.Add(new Wave { waveIndex = 5, wavePoints = 25, maxHitsAllowed = 8 });
        waves.Add(new Wave { waveIndex = 5, wavePoints = 26, maxHitsAllowed = 8 });
        waves.Add(new Wave { waveIndex = 5, wavePoints = 64, maxHitsAllowed = 7 });
        waves.Add(new Wave { waveIndex = 5, wavePoints = 79, maxHitsAllowed = 7 });
    }

    private void StartWave()
    {
        if (progressCoroutine != null)
        {
            StopCoroutine(progressCoroutine);
            progressCoroutine = null;
        }

        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("All waves complete");
            gameOver = true;
            WinScreen();
            return;
        }
        if (gameOver)
        {
            Debug.Log("Game is Over");
            return;
        }

        if (currentWaveIndex >= 5)
        {
            currentWaveTime = waveTimeLimit;
            UpdateTimerUI();
        }
        else
        {
            if (timerText != null)
                timerText.text = "";
        }

        waveStartTime = Time.time;

        Debug.Log($"Starting Wave {currentWaveIndex}");
        isSpawning = true;
        waveInProgress = true;
        failInProgress = false;
        playerHit.ResetHits();
        enemiesToSpawn.Clear(); // Clear previous wave enemies
        spawnedEnemies.Clear(); // Clear any lingering enemies

        playerHit.SetMaxHits(waves[currentWaveIndex].maxHitsAllowed);

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
            yield return new WaitForSeconds(1f);
            GameObject enemy = Instantiate(enemiesToSpawn[0], spawnLocation[spawnIndex].position, Quaternion.identity);
            spawnedEnemies.Add(enemy);
            enemiesToSpawn.RemoveAt(0);
            spawnIndex = (spawnIndex + 1) % spawnLocation.Length;
        }

        waveInProgress = false;
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
        progressCoroutine = null;
    }

    private IEnumerator WaveSuccess()
    {
        waveInProgress = false;
        upgradeInProgress = true;
        failInProgress = false;
        ShowDifficulty("Difficulty Raised");
        yield return new WaitForSeconds(2f);
        ShowUpgradePanel();
        upgradeInProgress = false;
    }

    private IEnumerator WaveFailure()
    {
        if (failInProgress) yield break;
        failInProgress = true;
        waveInProgress = false;
        ShowDifficulty("Difficulty Lowered");
        yield return new WaitForSeconds(2f);
        if (!string.IsNullOrEmpty(lastUpgradeType))
        {
            RevertLastUpgrade();
            lastUpgradeType = "";
        }
        if (currentWaveIndex > 0 )
        {
            currentWaveIndex--;
        }
        failInProgress = false;
        StartWave();
    }

    private IEnumerator HideDifficulty(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (difficultyIndicator != null )
        {
            difficultyIndicator.gameObject.SetActive(false);
        }
    }

    private void CheckWaveProgress()
    {
        if (spawnedEnemies.Count > 0)
        {
            Debug.Log("Enemies still exist! Waiting for them to be destroyed.");
            return; // Do NOT progress until all enemies are gone
        }

        if (playerHit.hitCount < waves[currentWaveIndex].maxHitsAllowed)
        {
            if (currentWaveIndex == waves.Count - 1)
            {
                Debug.Log("Final Wave Complete");
                gameOver = true;
                WinScreen();
            }
            else
            {
                StartCoroutine(WaveSuccess());
            }
        }
        else
        {
            StartCoroutine(WaveFailure());
        }
    }

    private void ShowUpgradePanel()
    {
        if (upgradePanel != null)
        {
            GameUpgrade = true;
            upgradeAnim.UpgradeIntro();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            upgradePanel.SetActive(true);
            if (pauseMenu != null)
            {
                pauseMenu.UpgradeActive = true;
            }    
        }
        else
        {
            Debug.LogWarning("Panel not Assigned");
            ApplyUpgrade("");
        }
    }

    private void ShowDifficulty(string message)
    {
        if (difficultyIndicator != null)
        {
            difficultyIndicator.text = message;
            difficultyIndicator.gameObject.SetActive(true);
            StartCoroutine(HideDifficulty(2f));
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
            upgradeAnim.UpgradeOutro();
            upgradePanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        GameUpgrade = false;
        if (pauseMenu != null)
        {
            pauseMenu.UpgradeActive = false;
        }
        Time.timeScale = 1f;
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

    private void WinScreen()
    {
        if (winScreen != null)
        {
            winAnim.WinIntro();
            winScreen.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.Ceil(currentWaveTime).ToString();
        }
    }
}


