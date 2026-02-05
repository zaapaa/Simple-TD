using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using TMPro;

public class EnemyWaveSpawner : MonoBehaviour
{
    public float timeBetweenWavesInitial = 60f;
    public float timeBetweenWavesDecrease = 1f;
    public float timeBetweenWavesMin = 5f;
    public float waveBudget = 10f;
    public float waveBudgetIncrease = 1f;
    public float enemyWaitIntervalInitial = 1f;
    public float enemyWaitIntervalDecrease = 0.02f;
    public float enemyWaitIntervalMin = 0.5f;
    public List<GameObject> enemyPrefabs;
    public Transform spawnPoint;
    public Transform endpoint;
    private float timeBetweenWaves;
    private int currentWave = 1;
    private float waveTimer;
    private float enemyWaitInterval;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeBetweenWaves = timeBetweenWavesInitial;
        waveTimer = timeBetweenWaves;
        enemyWaitInterval = enemyWaitIntervalInitial;
        StartNextWave();
    }

    // Update is called once per frame
    void Update()
    {

        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0f)
        {
            StartNextWave();
        }
        GameManager.instance.waveTimerText.GetComponent<TextMeshProUGUI>().text = waveTimer.ToString("F0");
    }

    public void StartNextWave()
    {
        StartCoroutine(SpawnWave());
        currentWave++;
        IncreaseWaveDifficulty();
        waveTimer = timeBetweenWaves;
    }

    private IEnumerator SpawnWave()
    {
        Dictionary<GameObject, int> enemySpawnCount = GetEnemySpawnCount();
        List<GameObject> randomSpawnWave = GetRandomizedSpawnWave(enemySpawnCount);
        foreach (var enemy in randomSpawnWave)
        {
            SpawnEnemy(enemy, spawnPoint.position);
            yield return new WaitForSeconds(enemyWaitInterval);
        }
    }
    public void SpawnEnemy(GameObject enemyPrefab, Vector3 spawnPosition)
    {
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, enemyPrefab.transform.rotation);
        enemy.transform.parent = transform;
        enemy.GetComponent<Enemy>().Initialize(spawnPoint, endpoint, this);
    }
    Dictionary<GameObject, int> GetEnemySpawnCount()
    {
        Dictionary<GameObject, int> waveEnemyCounts = new Dictionary<GameObject, int>();
        float waveBudgetLeft = waveBudget;
        float minWaveBudget = GetEnemyMinWaveBudget();
        float selectChance = 0.2f;
        while (waveBudgetLeft > 0)
        {
            foreach (GameObject enemy in enemyPrefabs)
            {
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                Debug.Assert(enemyScript.waveBudget > 0, "Enemy wave budget must be greater than 0");
                if (waveBudgetLeft >= enemyScript.waveBudget &&
                    currentWave >= enemyScript.minSpawnWaveNumber &&
                    Random.value < selectChance)
                {
                    waveEnemyCounts[enemy.gameObject] = (waveEnemyCounts.ContainsKey(enemy.gameObject)) ? waveEnemyCounts[enemy.gameObject] + 1 : 1;
                    waveBudgetLeft -= enemyScript.waveBudget;
                }
            }
            if (waveBudgetLeft < minWaveBudget)
            {
                break;
            }
        }

        string enemySpawnString = "";
        foreach (var enemy in waveEnemyCounts)
        {
            enemySpawnString += $"{enemy.Value} {enemy.Key.name}s, ";
        }

        Debug.Log($"Spawning {enemySpawnString}");
        return waveEnemyCounts;
    }
    float GetEnemyMinWaveBudget()
    {
        return enemyPrefabs.OrderBy(e => e.GetComponent<Enemy>().waveBudget).First().GetComponent<Enemy>().waveBudget;
    }
    List<GameObject> GetRandomizedSpawnWave(Dictionary<GameObject, int> enemySpawnCount)
    {
        List<GameObject> randomSpawnWave = new List<GameObject>();
        foreach (var enemy in enemySpawnCount)
        {
            for (int i = 0; i < enemy.Value; i++)
            {
                randomSpawnWave.Add(enemy.Key);
            }
        }
        // Modern Fisher-Yates shuffle (backward iteration)
        for (int i = randomSpawnWave.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (randomSpawnWave[i], randomSpawnWave[randomIndex]) = (randomSpawnWave[randomIndex], randomSpawnWave[i]);
        }
        return randomSpawnWave;
    }
    void IncreaseWaveDifficulty()
    {
        timeBetweenWaves -= timeBetweenWavesDecrease;
        timeBetweenWaves = Mathf.Max(timeBetweenWaves, timeBetweenWavesMin);
        waveBudget += waveBudgetIncrease;
        enemyWaitInterval -= enemyWaitIntervalDecrease;
        enemyWaitInterval = Mathf.Max(enemyWaitInterval, enemyWaitIntervalMin);
    }
}
