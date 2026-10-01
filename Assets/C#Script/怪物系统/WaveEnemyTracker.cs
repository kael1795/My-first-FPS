using UnityEngine;

public class WaveEnemyTracker : MonoBehaviour, IPoolable
{
    public Enemybase Enemy { get; private set; }

    private bool removed;

    public void Initialize(Enemybase enemy)
    {
        Enemy = enemy;
        removed = false;
    }

    public void OnSpawn()
    {
        removed = false;
    }

    public void OnDespawn()
    {
        NotifyRemoved();
    }

    public void NotifyRemoved()
    {
        if (removed)
            return;

        removed = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyEnemyRemoved(this);
        }
    }

    private void OnDestroy()
    {
        NotifyRemoved();
    }
}