using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    #region Boss Health UI
    public GameObject bossBar;
    public BossHealthbar bossHealthBar;
    private BossEnemy currentBoss;
    #endregion

    #region Audio
    private AudioSource audioSource;
    [SerializeField] AudioClip difUp;
    [SerializeField] AudioClip difDown;
    #endregion

    private void Start()
    {
        audioSource = GetComponentInParent<AudioSource>();
        InitializeWaves(); //Set Up wave data
        bossBar.SetActive(false); //Hide boss bar initially
        StartWave(); //Start wave
    }

    private void Update()
    {
        if (!gameOver && !GameUpgrade)
        {
            if (currentWaveIndex >= 5) //If current wave is timed
            {

                currentWaveTime -= Time.deltaTime; //Count down

                if (currentWaveTime < 0)
                {
                    currentWaveTime = 0;
                }
                UpdateTimerUI(); //Show updated timer

                if (currentWaveTime <= 0 && (spawnedEnemies.Count > 0 || enemiesToSpawn.Count > 0) //Fail Condition
                    && !failInProgress && !upgradeInProgress)
                {
                    StartCoroutine(WaveFailure());
                }
            }
            else
            {
                if (timerText != null) //Clear timer text for early waves
                {
                    timerText.text = "";
                }
            }
        }
    }
    private void FixedUpdate()
    {
        spawnedEnemies.RemoveAll(item => item == null); //clean up destroyed enemies

        if (spawnTimer <= 0 && isSpawning && enemiesToSpawn.Count > 0)
        {
            StartCoroutine(DelaySpawn()); //Start Spawning
            spawnTimer = 2f; //Reset timer
        }
        else
        {
            spawnTimer -= Time.fixedDeltaTime; //Count down spawn timer
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
        waves.Clear(); //Fresh Lists
        waves.Add(new Wave { waveIndex = 0, wavePoints = 1, maxHitsAllowed = 10 });
        waves.Add(new Wave { waveIndex = 1, wavePoints = 3, maxHitsAllowed = 10 });
        waves.Add(new Wave { waveIndex = 2, wavePoints = 4, maxHitsAllowed = 10 });
        waves.Add(new Wave { waveIndex = 3, wavePoints = 7, maxHitsAllowed = 9 });
        waves.Add(new Wave { waveIndex = 4, wavePoints = 9, maxHitsAllowed = 9 });
        waves.Add(new Wave { waveIndex = 5, wavePoints = 25, maxHitsAllowed = 8 });
        waves.Add(new Wave { waveIndex = 6, wavePoints = 26, maxHitsAllowed = 8 });
        waves.Add(new Wave { waveIndex = 7, wavePoints = 64, maxHitsAllowed = 7 });
        waves.Add(new Wave { waveIndex = 8, wavePoints = 79, maxHitsAllowed = 7 });
        waves.Add(new Wave { waveIndex = 9, wavePoints = 80, maxHitsAllowed = 5 });
    }

    private void StartWave()
    {
        if (progressCoroutine != null)
        {
            StopCoroutine(progressCoroutine); //Stop any active progress checking
            progressCoroutine = null;
        }

        if (currentWaveIndex >= waves.Count) //If all waves are done
        {
            Debug.Log("All waves complete");
            gameOver = true;
            WinScreen(); //Show Win Screen
            return;
        }

        if (gameOver) //If game already ended
        {
            Debug.Log("Game is Over");
            return;
        }

        if (currentWaveIndex >= 5) //Start wave timer if wave index is 5 or higher
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

        playerHit.SetMaxHits(waves[currentWaveIndex].maxHitsAllowed); //Set wave hit limit

        GenerateEnemies(waves[currentWaveIndex].wavePoints); //Create enemies for this wave

        if (enemiesToSpawn.Count == 0) //If something is wrong and no enemies were generated
        {
            Debug.LogError($"Wave {currentWaveIndex} Failed - No enemies generated!");
            return;
        }

        Debug.Log($"Wave {currentWaveIndex}: Spawning {enemiesToSpawn.Count} enemies.");

        if (currentWaveIndex == waves.Count - 1) //Show boss healthbar
        {
            bossBar.SetActive(true);
        }
        else
        {
            bossBar.SetActive(false);
        }
    }

    private void GenerateEnemies(int wavePoints)
    {
        List<GameObject> generatedEnemies = new List<GameObject>(); //Temp list for generated enemies

        int remainingPoints = wavePoints; //Remaining cost points to spend

        Debug.Log($"Generating Wave {currentWaveIndex}: {wavePoints} points available.");

        while (remainingPoints > 0)
        {
            EnemySpawn selectedEnemy = null;

            for (int i = enemies.Count - 1; i >= 0; i--) //Start from most expensive to least and spend points efficiently
            {
                if (enemies[i].cost <= remainingPoints)
                {
                    selectedEnemy = enemies[i];
                    break;
                }
            }

            if (selectedEnemy == null) //If no enemy fits in the remaining cost, stop loop
            {
                Debug.LogWarning($"Wave {currentWaveIndex + 1}: No enemy found for remaining points ({remainingPoints}).");
                break;
            }

            generatedEnemies.Add(selectedEnemy.enemyPrefab); //Add select enemies to list and subtract cost
            remainingPoints -= selectedEnemy.cost;

            Debug.Log($"Wave {currentWaveIndex + 1}: Spawned {selectedEnemy.enemyPrefab.name} (Cost: {selectedEnemy.cost}, Remaining Points: {remainingPoints})");
        }

        if (generatedEnemies.Count == 0)
        {
            Debug.LogError($"Wave {currentWaveIndex + 1} failed! No enemies generated.");
        }

        enemiesToSpawn = generatedEnemies; //store list for spawning
        isSpawning = true; //Enable spawn bool
    }

    //Coroutine to spawn enemies over time from enemiesToSpawn list
    private IEnumerator DelaySpawn()
    {
        isSpawning = false;
        while (enemiesToSpawn.Count > 0)
        {
            yield return new WaitForSeconds(1f); //Delay between each enemy

            GameObject enemy = Instantiate(enemiesToSpawn[0], spawnLocation[spawnIndex].position, Quaternion.identity); //Instantiate enemy
            spawnedEnemies.Add(enemy); //Track spawned enemy
            enemiesToSpawn.RemoveAt(0); //Remove from list
            spawnIndex = (spawnIndex + 1) % spawnLocation.Length; //Cycle through spawn points

            if (currentWaveIndex == waves.Count - 1) //If boss wave, bind the boss health UI
            {
                BossEnemy boss = enemy.GetComponent<BossEnemy>();
                if (boss != null)
                {
                    currentBoss = boss;
                    bossHealthBar.SetBoss(boss);
                }
            }
        }

        waveInProgress = false;
    }

    //Coroutine to wait for checking wave status
    private IEnumerator WaitAndCheckWaveProgress()
    {
        checkingProgress = true;
        int checkWaveIndex = currentWaveIndex;

        yield return new WaitUntil(() => Time.time - waveStartTime >= minWaveDuration); //Wait until minimum wave duration has passed
        yield return new WaitForSeconds(1f); //Small delay
        if (currentWaveIndex == checkWaveIndex) 
        {
            CheckWaveProgress();
        }
        checkingProgress = false;
        progressCoroutine = null;
    }

    //Coroutine for Wave Success
    private IEnumerator WaveSuccess()
    {
        waveInProgress = false;
        upgradeInProgress = true;
        failInProgress = false;
        ShowDifficulty("Difficulty Raised"); //Show feedback
        GetComponentInParent<AudioSource>().PlayOneShot(difUp); 
        yield return new WaitForSeconds(2f);
        ShowUpgradePanel(); //Open upgrade UI
        upgradeInProgress = false;
    }

    //Coroutine for Wave Failure
    private IEnumerator WaveFailure()
    {
        if (failInProgress) yield break;
        failInProgress = true;
        waveInProgress = false;

        ShowDifficulty("Difficulty Lowered"); //Show Feedback
        GetComponentInParent<AudioSource>().PlayOneShot(difDown);
        yield return new WaitForSeconds(2f);

        if (!string.IsNullOrEmpty(lastUpgradeType)) //Revert upgrade if player failed
        {
            RevertLastUpgrade();
            lastUpgradeType = "";
        }
        if (currentWaveIndex > 0 )
        {
            currentWaveIndex--; //drop previous wave
        }
        failInProgress = false;
        StartWave(); //retry the previous wave
    }

    //Hides the difficulty indicator text after delay
    private IEnumerator HideDifficulty(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (difficultyIndicator != null )
        {
            difficultyIndicator.gameObject.SetActive(false);
        }
    }
    //Checks player performance to determine win/loss
    private void CheckWaveProgress()
    {
        if (spawnedEnemies.Count > 0)
        {
            Debug.Log("Enemies still exist! Waiting for them to be destroyed.");
            return; // Do NOT progress until all enemies are gone
        }

        if (playerHit.hitCount < waves[currentWaveIndex].maxHitsAllowed) //If player did well
        {
            if (currentWaveIndex == waves.Count - 1) //If it is the last wave, trigger win screen
            {
                Debug.Log("Final Wave Complete");
                gameOver = true;
                bossBar.SetActive(false);
                WinScreen();
            }
            else
            {
                StartCoroutine(WaveSuccess());
            }
        }
        else
        {
            StartCoroutine(WaveFailure()); //Too many hits
        }
    }

    //Opens upgrade panel
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
            ApplyUpgrade(""); //Skip Upgrade if no panel
        }
    }

    //Shows temporary message when difficulty changes
    private void ShowDifficulty(string message)
    {
        if (difficultyIndicator != null)
        {
            difficultyIndicator.text = message;
            difficultyIndicator.gameObject.SetActive(true);
            StartCoroutine(HideDifficulty(2f));
        }
    }

   //Called when player selects an upgrade
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

        if (upgradePanel != null) //Closes upgrade panel and resume game
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
        currentWaveIndex++; //Advance to next wave
        StartWave();
    }

    //Reverts previously chosen upgrade if player fails
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

    //Displays win screen and freezes game
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
    //Update the wave timer UI
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.Ceil(currentWaveTime).ToString();
        }
    }
}


