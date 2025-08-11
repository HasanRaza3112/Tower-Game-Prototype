using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }
    
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform enemyParent;
    
    [Header("Wave Settings")]
    [SerializeField] private int enemiesPerWave = 10;
    [SerializeField] private float timeBetweenSpawns = 1f;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float waveHealthMultiplier = 1.2f;
    
    private int currentWave = 0;
    private bool isSpawning = false;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine spawnCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        if (enemyParent == null)
        {
            enemyParent = new GameObject("Enemies").transform;
        }
        
        StartNextWave();
    }
    
    public void StartNextWave()
    {
        if (isSpawning) return;
        
        currentWave++;
        spawnCoroutine = StartCoroutine(SpawnWave());
    }
    
    private IEnumerator SpawnWave()
    {
        isSpawning = true;
        
        int enemiesToSpawn = enemiesPerWave + (currentWave - 1) * 2; // Increase enemies each wave
        
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
        
        isSpawning = false;
        
        // Wait for wave to complete before starting next one
        StartCoroutine(CheckWaveComplete());
    }
    
    private IEnumerator CheckWaveComplete()
    {
        while (activeEnemies.Count > 0)
        {
            // Clean up null references
            activeEnemies.RemoveAll(enemy => enemy == null);
            yield return new WaitForSeconds(1f);
        }
        
        yield return new WaitForSeconds(timeBetweenWaves);
        StartNextWave();
    }
    
    private void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;
        
        // Choose random enemy and spawn point
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation, enemyParent);
        
        // Scale enemy health based on wave
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            float healthMultiplier = Mathf.Pow(waveHealthMultiplier, currentWave - 1);
            // You'll need to modify EnemyHealth to accept health scaling
        }
        
        activeEnemies.Add(enemy);
    }
    
    public void RemoveEnemy(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
    }
    
    public int GetCurrentWave()
    {
        return currentWave;
    }
    
    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }
    
    private void OnDestroy()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
}
