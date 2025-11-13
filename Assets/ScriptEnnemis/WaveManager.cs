using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public EnemyWaveData[] waves;
    public int currentWaveIndex = 0;
    public bool autoStartFirstWave = true;
    public float delayBeforeNextWave = 3f;
    
    [Header("Spawn Settings")]
    public Transform player;
    public float spawnDistance = 10f;
    public float spawnHeight = 1f;
    public bool autoFindPlayer = true;
    
    [Header("Spawn Area")]
    public Vector2 spawnAreaMin = new Vector2(-15f, -15f);
    public Vector2 spawnAreaMax = new Vector2(15f, 15f);
    
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;
    private bool waveActive = false;
    
    private void Start()
    {
        if (autoFindPlayer)
        {
            FindPlayer();
        }
        
        if (autoStartFirstWave && waves.Length > 0)
        {
            StartCoroutine(StartWaveAfterDelay(1f));
        }
    }
    
    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                player = mainCamera.transform;
            }
        }
    }
    
    private void Update()
    {
        CleanUpDestroyedEnemies();
        
        if (waveActive && activeEnemies.Count == 0 && !isSpawning)
        {
            OnWaveComplete();
        }
    }
    
    private void CleanUpDestroyedEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }
    
    public void StartWave(int waveIndex)
    {
        if (waveIndex >= 0 && waveIndex < waves.Length && !isSpawning)
        {
            currentWaveIndex = waveIndex;
            StartCoroutine(SpawnWave(waves[waveIndex]));
        }
    }
    
    public void StartNextWave()
    {
        if (currentWaveIndex < waves.Length - 1)
        {
            currentWaveIndex++;
            StartWave(currentWaveIndex);
        }
        else
        {
            Debug.Log("All waves completed!");
        }
    }
    
    private IEnumerator StartWaveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartWave(currentWaveIndex);
    }
    
    private IEnumerator SpawnWave(EnemyWaveData wave)
    {
        isSpawning = true;
        waveActive = true;
        
        Debug.Log($"Starting Wave {wave.waveNumber}");
        
        foreach (EnemySpawnInfo spawnInfo in wave.enemies)
        {
            for (int i = 0; i < spawnInfo.count; i++)
            {
                SpawnEnemy(spawnInfo.enemyPrefab);
                yield return new WaitForSeconds(wave.timeBetweenSpawns);
            }
        }
        
        isSpawning = false;
    }
    
    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null || player == null)
        {
            return;
        }
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(enemy);
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 playerPos = player.position;
        
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomZ = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        
        Vector3 offset = new Vector3(randomX, 0f, randomZ);
        Vector3 spawnPos = playerPos + offset;
        spawnPos.y = playerPos.y + spawnHeight;
        
        return spawnPos;
    }
    
    private void OnWaveComplete()
    {
        waveActive = false;
        Debug.Log($"Wave {waves[currentWaveIndex].waveNumber} Complete!");
        
        if (currentWaveIndex < waves.Length - 1)
        {
            StartCoroutine(StartWaveAfterDelay(delayBeforeNextWave));
        }
        else
        {
            Debug.Log("Victory! All waves completed!");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (player == null)
        {
            return;
        }
        
        Vector3 playerPos = player.position;
        
        Gizmos.color = Color.yellow;
        Vector3 min = new Vector3(playerPos.x + spawnAreaMin.x, playerPos.y, playerPos.z + spawnAreaMin.y);
        Vector3 max = new Vector3(playerPos.x + spawnAreaMax.x, playerPos.y, playerPos.z + spawnAreaMax.y);
        
        Vector3 size = max - min;
        Vector3 center = min + size * 0.5f;
        
        Gizmos.DrawWireCube(center, new Vector3(size.x, 0.1f, size.z));
    }
}
