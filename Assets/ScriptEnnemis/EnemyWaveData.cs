using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public int count;
}

[CreateAssetMenu(fileName = "Wave", menuName = "Game/Enemy Wave")]
public class EnemyWaveData : ScriptableObject
{
    public int waveNumber;
    public EnemySpawnInfo[] enemies;
    public float timeBetweenSpawns = 0.5f;
}