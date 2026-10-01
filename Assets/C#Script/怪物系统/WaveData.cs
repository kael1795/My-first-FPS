using System;
using UnityEngine;

[Serializable]
public class WaveData
{
    public string waveName = "Wave";

    [Min(1)]
    public int enemyCount = 5;

    [Min(0.05f)]
    public float spawnInterval = 1f;

    [Min(0f)]
    public float delayBeforeWave = 3f;

    public bool waitUntilAllEnemiesDead = true;
}
