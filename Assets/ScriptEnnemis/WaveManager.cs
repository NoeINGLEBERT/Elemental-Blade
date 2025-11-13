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
    
    [Header("Spawn Center")]
    public SpawnCenterMode centerMode = SpawnCenterMode.Player;
    public Transform player;
    public Transform customSpawnCenter;
    public Vector3 fixedSpawnCenter = Vector3.zero;
    public bool autoFindPlayer = true;
    
    [Header("Spawn Settings")]
    public float spawnHeight = 1f;
    
    [Header("Circular Spawn Area")]
    public float minSpawnRadius = 10f;
    public float maxSpawnRadius = 15f;
    
    public enum SpawnCenterMode
    {
        Player,
        CustomTransform,
        FixedPosition,
        WaveManagerPosition
    }
    
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
        if (enemyPrefab == null)
        {
            return;
        }
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(enemy);
    }
    
    private Vector3 GetSpawnCenter()
    {
        switch (centerMode)
        {
            case SpawnCenterMode.Player:
                return player != null ? player.position : Vector3.zero;
            
            case SpawnCenterMode.CustomTransform:
                return customSpawnCenter != null ? customSpawnCenter.position : Vector3.zero;
            
            case SpawnCenterMode.FixedPosition:
                return fixedSpawnCenter;
            
            case SpawnCenterMode.WaveManagerPosition:
                return transform.position;
            
            default:
                return Vector3.zero;
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 centerPos = GetSpawnCenter();
        
        float randomAngle = Random.Range(0f, 360f);
        float randomRadius = Random.Range(minSpawnRadius, maxSpawnRadius);
        
        float x = Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomRadius;
        float z = Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomRadius;
        
        Vector3 spawnPos = centerPos + new Vector3(x, spawnHeight, z);
        
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
        Vector3 centerPos = GetSpawnCenter();
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        DrawCircle(centerPos, maxSpawnRadius, 64);
        
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        DrawCircle(centerPos, minSpawnRadius, 64);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPos, 0.5f);
    }
    
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
