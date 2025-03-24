using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    public List<EnemySpawn> enemies = new List<EnemySpawn>();
    public int currWave;
    public int waveValue;
    public List<GameObject> enemiesToSpawn = new List<GameObject>();

    public Transform[] spawnLocation;
    public int spawnIndex; 

    public int waveDuration;
    private float waveTimer;
    [SerializeField] float spawnInterval = 2f;
    private float spawnTimer;

    private bool isSpawning = false;

    public List<GameObject> spawnedEnemies = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        GenerateWave();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        spawnedEnemies.RemoveAll(item => item == null); //removes all items where an item is null
        if (spawnTimer <= 0)
        {
            //spawn enemy
            if (isSpawning)
            {
                StartCoroutine(DelaySpawn(waveValue)); // spawn first enemy in our list
                spawnTimer = spawnInterval;

                if (spawnIndex + 1 <= spawnLocation.Length - 1)
                {
                    spawnIndex++;
                }
                else
                {
                    spawnIndex = 0;
                }
            }
            else
            {
                waveTimer = 0; //if no enemies remain, end wave
            }
        }
        else
        {
            spawnTimer -= Time.fixedDeltaTime;
        }

        if (waveTimer <= 0 && spawnedEnemies.Count <= 0)
        {
            currWave++;
            GenerateWave();
            waveTimer -= Time.fixedDeltaTime;
        }
    }

    public void GenerateWave()
    {
        waveValue = currWave * 2; //adds to the 
        GenerateEnemies();
        spawnInterval = 2f; // gives a fixed time between each enemies
        waveTimer = waveDuration; // wave duration is read only
    }

    public void GenerateEnemies()
    {
        //Create Temporary list of enemies
        //in a loop grab a random enemy
        //see if we can afford it
        //if we can, add it to list and deduct cost
        //repeat
        //if no points leave loop

        List<GameObject> generatedEnemies = new List<GameObject>();
        while (waveValue > 0 || generatedEnemies.Count < 50)
        {
            int randEnemyId = Random.Range(0, enemies.Count);
            int randEnemyCost = enemies[randEnemyId].cost;

            if (waveValue - randEnemyCost >= 0)
            {
                generatedEnemies.Add(enemies[randEnemyId].enemyPrefab);
                waveValue -= randEnemyCost;
            }
            else if (waveValue <= 0)
            {
                break;
            }
        }
        enemiesToSpawn.Clear();
        enemiesToSpawn = generatedEnemies;
        isSpawning = true;
    }

    private IEnumerator DelaySpawn (int enemyValue)
    {
        isSpawning = false;

        int spawnCount = 0;
        while (enemiesToSpawn.Count > 0)
        {
            yield return new WaitForSeconds(spawnInterval);
            spawnCount++;
            GameObject enemy = (GameObject)Instantiate(enemiesToSpawn[0], spawnLocation[spawnIndex].position, Quaternion.identity);
            spawnedEnemies.Add(enemy);
            enemiesToSpawn.RemoveAt(0);
        }

    }
}
[System.Serializable]
public class EnemySpawn
{
    public GameObject enemyPrefab;
    public int cost;
}

