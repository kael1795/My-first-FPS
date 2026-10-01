using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Spawn Points")]
    [SerializeField]
    private List<WaveSpawnPoint> spawnPoints =
        new List<WaveSpawnPoint>();

    [SerializeField] private bool autoCollectSpawnPoints = true;

    [Header("Wave Settings")]
    [SerializeField] private WaveData[] waves = new WaveData[0];
    [SerializeField] private bool autoStart = true;

    [Min(1)]
    [SerializeField] private int baseEnemyCount = 5;

    [Min(0)]
    [SerializeField] private int enemyCountIncreasePerWave = 3;

    [Min(0.05f)]
    [SerializeField] private float baseSpawnInterval = 1f;

    [Min(0f)]
    [SerializeField] private float spawnIntervalReductionPerWave = 0.05f;

    [Min(0.05f)]
    [SerializeField] private float minSpawnInterval = 0.25f;

    [SerializeField] private bool showDebugLogs = true;

    private readonly HashSet<WaveEnemyTracker> activeEnemies =
        new HashSet<WaveEnemyTracker>();

    private Coroutine waveCoroutine;
    private int nextSpawnPointIndex;
    private int currentWaveIndex = -1;
    private int spawnedThisWave;

    public bool IsRunning { get; private set; }
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int ActiveEnemyCount => activeEnemies.Count;

    public event Action<int> WaveStarted;
    public event Action<int> WaveCompleted;
    public event Action<int, int, int> WaveProgressChanged;
    public event Action<WaveEnemyTracker> EnemySpawned;
    public event Action<WaveEnemyTracker> EnemyRemoved;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RefreshSpawnPoints();

        if (autoStart)
        {
            StartWaves();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void StartWaves()
    {
        if (IsRunning)
            return;

        RefreshSpawnPoints();

        if (spawnPoints.Count == 0)
        {
            Debug.LogError(
                "[GameManager] 没有可用的怪物刷新点。",
                this
            );
            return;
        }

        waveCoroutine = StartCoroutine(WaveRoutine());
    }

    public void StopWaves(bool destroyActiveEnemies)
    {
        IsRunning = false;

        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        if (!destroyActiveEnemies)
            return;

        List<WaveEnemyTracker> enemiesToDestroy =
            new List<WaveEnemyTracker>(activeEnemies);

        activeEnemies.Clear();

        foreach (WaveEnemyTracker enemy in enemiesToDestroy)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        NotifyWaveProgress();
    }

    public void RegisterSpawnPoint(WaveSpawnPoint spawnPoint)
    {
        if (spawnPoint == null || !spawnPoint.HasValidEnemyPrefab)
            return;

        if (!spawnPoints.Contains(spawnPoint))
        {
            spawnPoints.Add(spawnPoint);
        }
    }

    public void UnregisterSpawnPoint(WaveSpawnPoint spawnPoint)
    {
        if (spawnPoint != null)
        {
            spawnPoints.Remove(spawnPoint);
        }
    }

    public void RefreshSpawnPoints()
    {
        if (autoCollectSpawnPoints)
        {
            spawnPoints = new List<WaveSpawnPoint>(
                FindObjectsOfType<WaveSpawnPoint>()
            );
        }

        for (int i = spawnPoints.Count - 1; i >= 0; i--)
        {
            WaveSpawnPoint spawnPoint = spawnPoints[i];

            if (spawnPoint == null || !spawnPoint.HasValidEnemyPrefab)
            {
                spawnPoints.RemoveAt(i);
            }
        }
    }

    private IEnumerator WaveRoutine()
    {
        IsRunning = true;
        int waveIndex = 0;

        while (IsRunning)
        {
            WaveData wave = GetWaveData(waveIndex);

            if (wave.delayBeforeWave > 0f)
            {
                yield return new WaitForSeconds(wave.delayBeforeWave);
            }

            if (!IsRunning)
                break;

            currentWaveIndex = waveIndex;
            spawnedThisWave = 0;

            if (showDebugLogs)
            {
                Debug.Log(
                    $"[GameManager] 开始 {wave.waveName}，" +
                    $"怪物数量：{wave.enemyCount}"
                );
            }

            WaveStarted?.Invoke(waveIndex + 1);
            NotifyWaveProgress();

            for (int i = 0; i < wave.enemyCount; i++)
            {
                WaveSpawnPoint spawnPoint = GetNextSpawnPoint();

                if (spawnPoint == null)
                {
                    Debug.LogError(
                        "[GameManager] 没有有效刷新点，停止生成。",
                        this
                    );

                    IsRunning = false;
                    break;
                }

                WaveEnemyTracker enemy = spawnPoint.Spawn();

                if (enemy != null)
                {
                    activeEnemies.Add(enemy);
                    spawnedThisWave++;

                    EnemySpawned?.Invoke(enemy);
                    NotifyWaveProgress();
                }

                if (i < wave.enemyCount - 1 &&
                    wave.spawnInterval > 0f)
                {
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }

            if (!IsRunning)
                break;

            if (wave.waitUntilAllEnemiesDead)
            {
                while (IsRunning && activeEnemies.Count > 0)
                {
                    yield return null;
                }
            }

            if (!IsRunning)
                break;

            if (showDebugLogs)
            {
                Debug.Log($"[GameManager] {wave.waveName} 已完成");
            }

            WaveCompleted?.Invoke(waveIndex + 1);
            waveIndex++;
        }

        IsRunning = false;
        waveCoroutine = null;
    }

    private WaveData GetWaveData(int waveIndex)
    {
        if (waves != null && waveIndex < waves.Length)
        {
            return waves[waveIndex];
        }

        return new WaveData
        {
            waveName = "Wave " + (waveIndex + 1),
            enemyCount = baseEnemyCount +
                enemyCountIncreasePerWave * waveIndex,
            spawnInterval = Mathf.Max(
                minSpawnInterval,
                baseSpawnInterval -
                spawnIntervalReductionPerWave * waveIndex
            ),
            delayBeforeWave = 3f,
            waitUntilAllEnemiesDead = true
        };
    }

    private WaveSpawnPoint GetNextSpawnPoint()
    {
        if (spawnPoints.Count == 0)
            return null;

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            int index = (nextSpawnPointIndex + i) % spawnPoints.Count;
            WaveSpawnPoint spawnPoint = spawnPoints[index];

            if (spawnPoint != null && spawnPoint.HasValidEnemyPrefab)
            {
                nextSpawnPointIndex =
                    (index + 1) % spawnPoints.Count;

                return spawnPoint;
            }
        }

        return null;
    }

    // 支持在EnemyBase的死亡方法中直接调用
    public void NotifyEnemyDied(Enemybase enemy)
    {
        if (enemy == null)
            return;

        WaveEnemyTracker tracker =
            enemy.GetComponentInParent<WaveEnemyTracker>();

        if (tracker != null)
        {
            RemoveActiveEnemy(tracker);
        }
    }

    public void NotifyEnemyRemoved(WaveEnemyTracker enemy)
    {
        RemoveActiveEnemy(enemy);
    }

    private void RemoveActiveEnemy(WaveEnemyTracker enemy)
    {
        if (enemy == null)
            return;

        if (activeEnemies.Remove(enemy))
        {
            EnemyRemoved?.Invoke(enemy);
            NotifyWaveProgress();
        }
    }

    private void NotifyWaveProgress()
    {
        WaveProgressChanged?.Invoke(
            CurrentWaveNumber,
            spawnedThisWave,
            activeEnemies.Count
        );
    }
}