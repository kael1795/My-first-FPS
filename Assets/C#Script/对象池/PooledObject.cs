using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [SerializeField] private GameObject originPrefab;

    public GameObject OriginPrefab => originPrefab;

    internal void SetOriginPrefab(GameObject prefab)
    {
        originPrefab = prefab;
    }

    public void Release()
    {
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
