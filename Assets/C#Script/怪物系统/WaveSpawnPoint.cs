using UnityEngine;

public class WaveSpawnPoint : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform enemyParent;

    [Header("Spawn Position")]
    [Min(0f)]
    [SerializeField] private float spawnRadius = 0f;

    [SerializeField] private bool randomRotation = true;

    public GameObject EnemyPrefab => enemyPrefab;

    public bool HasValidEnemyPrefab
    {
        get
        {
            if (enemyPrefab == null)
                return false;

            return enemyPrefab.GetComponentInChildren<Enemybase>(true) != null;
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterSpawnPoint(this);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterSpawnPoint(this);
        }
    }

    public WaveEnemyTracker Spawn()
    {
        if (!HasValidEnemyPrefab)
        {
            Debug.LogError(
                $"[WaveSpawnPoint] {name} 的怪物预制体没有EnemyBase组件。",
                this
            );
            return null;
        }

        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;

        Vector3 spawnPosition = transform.position + new Vector3(
            randomOffset.x,
            0f,
            randomOffset.y
        );

        Quaternion spawnRotation;

        if (randomRotation)
        {
            float randomY = transform.eulerAngles.y + Random.Range(0f, 360f);
            spawnRotation = Quaternion.Euler(0f, randomY, 0f);
        }
        else
        {
            spawnRotation = transform.rotation;
        }

        //GameObject enemyObject = Instantiate(
        //    enemyPrefab,
         //   spawnPosition,
         //   spawnRotation,
          //  enemyParent
       // );
        GameObject enemyObject = ObjectPoolManager.Instance.Spawn(
            enemyPrefab,
            spawnPosition,
            spawnRotation,
            enemyParent
        );
        Enemybase enemy = enemyObject.GetComponentInChildren< Enemybase>(true);

        if (enemy == null)
        {
            Destroy(enemyObject);
            return null;
        }

        WaveEnemyTracker tracker =
            enemyObject.GetComponent<WaveEnemyTracker>();

        if (tracker == null)
        {
            tracker = enemyObject.AddComponent<WaveEnemyTracker>();
        }

        tracker.Initialize(enemy);
        return tracker;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}