using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [Tooltip("每种预制体最多缓存多少个，0表示不限制")]
    [Min(0)]
    [SerializeField] private int defaultMaxPoolSize = 200;

    private readonly Dictionary<GameObject, Queue<GameObject>> pools =
        new Dictionary<GameObject, Queue<GameObject>>();

    private readonly Dictionary<GameObject, GameObject> instanceToPrefab =
        new Dictionary<GameObject, GameObject>();

    private readonly HashSet<GameObject> activeInstances =
        new HashSet<GameObject>();

    private Transform poolRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        GameObject rootObject = new GameObject("[PooledObjects]");
        poolRoot = rootObject.transform;
        poolRoot.SetParent(transform, false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public GameObject Spawn(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation,
        Transform parent = null
    )
    {
        if (prefab == null)
            return null;

        Queue<GameObject> pool = GetPool(prefab);
        GameObject instanceObject = null;

        while (pool.Count > 0)
        {
            instanceObject = pool.Dequeue();

            if (instanceObject != null)
                break;
        }

        if (instanceObject == null)
        {
            instanceObject = Instantiate(
                prefab,
                position,
                rotation,
                parent
            );

            PooledObject pooledObject =
                instanceObject.GetComponent<PooledObject>();

            if (pooledObject == null)
            {
                pooledObject =
                    instanceObject.AddComponent<PooledObject>();
            }

            pooledObject.SetOriginPrefab(prefab);
            instanceToPrefab[instanceObject] = prefab;
        }
        else
        {
            instanceObject.transform.SetParent(parent, false);
            instanceObject.transform.SetPositionAndRotation(
                position,
                rotation
            );

            instanceObject.SetActive(true);
        }

        activeInstances.Add(instanceObject);
        InvokeOnSpawn(instanceObject);

        return instanceObject;
    }

    public void Release(GameObject instanceObject)
    {
        if (instanceObject == null)
            return;

        if (!instanceToPrefab.TryGetValue(
            instanceObject,
            out GameObject prefab))
        {
            Destroy(instanceObject);
            return;
        }

        // 防止重复归还
        if (!activeInstances.Remove(instanceObject))
            return;

        InvokeOnDespawn(instanceObject);

        instanceObject.SetActive(false);
        instanceObject.transform.SetParent(poolRoot, false);

        Queue<GameObject> pool = GetPool(prefab);

        if (defaultMaxPoolSize > 0 &&
            pool.Count >= defaultMaxPoolSize)
        {
            instanceToPrefab.Remove(instanceObject);
            Destroy(instanceObject);
            return;
        }

        pool.Enqueue(instanceObject);
    }

    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
            return;

        Queue<GameObject> pool = GetPool(prefab);

        for (int i = 0; i < count; i++)
        {
            if (defaultMaxPoolSize > 0 &&
                pool.Count >= defaultMaxPoolSize)
            {
                break;
            }

            GameObject instanceObject = Instantiate(
                prefab,
                poolRoot
            );

            instanceObject.SetActive(false);

            PooledObject pooledObject =
                instanceObject.GetComponent<PooledObject>();

            if (pooledObject == null)
            {
                pooledObject =
                    instanceObject.AddComponent<PooledObject>();
            }

            pooledObject.SetOriginPrefab(prefab);
            instanceToPrefab[instanceObject] = prefab;
            pool.Enqueue(instanceObject);
        }
    }

    private Queue<GameObject> GetPool(GameObject prefab)
    {
        if (!pools.TryGetValue(
            prefab,
            out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pools.Add(prefab, pool);
        }

        return pool;
    }

    private void InvokeOnSpawn(GameObject instanceObject)
    {
        MonoBehaviour[] behaviours =
            instanceObject.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IPoolable poolable)
            {
                poolable.OnSpawn();
            }
        }
    }

    private void InvokeOnDespawn(GameObject instanceObject)
    {
        MonoBehaviour[] behaviours =
            instanceObject.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IPoolable poolable)
            {
                poolable.OnDespawn();
            }
        }
    }
}