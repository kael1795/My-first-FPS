using UnityEngine;

public class BulletControl : MonoBehaviour, IPoolable
{
    [Header("Bullet")]
    public float speed = 300f;
    public float aliveTime = 0f;

    [Min(0.05f)]
    public float lifeTime = 2f;

    public float damage=10f;
    public GameObject effectPrefab;

    [Header("Pool")]
    public PooledObject pooledObject;

    private Rigidbody rb;
    private Vector3 dir;
    private bool launched;
    public GameObject Player;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (pooledObject == null)
        {
            pooledObject = GetComponentInParent<PooledObject>();
        }
    }

    public void OnSpawn()
    {
        if (pooledObject == null)
        {
            pooledObject = GetComponentInParent<PooledObject>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        aliveTime = 0f;
        damage = 0f;
        launched = false;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void SetDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        dir = direction.normalized;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb == null)
        {
            Debug.LogError("BulletControl没有找到Rigidbody。", this);
            return;
        }

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        transform.forward = dir;
        rb.velocity = dir * speed;
        rb.angularVelocity = Vector3.zero;

        aliveTime = 0f;
        launched = true;
    }

    public void OnDespawn()
    {
        aliveTime = 0f;
        damage = 0f;
        launched = false;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void Update()
    {
        if (!launched || pooledObject == null)
            return;

        aliveTime += Time.deltaTime;

        if (aliveTime >= lifeTime)
        {
            launched = false;
            pooledObject.Release();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!launched)
            return;

        // 避免子弹刚生成时撞到玩家自己
        if (collision.gameObject.CompareTag("Player"))
            return;

        launched = false;

        if (effectPrefab != null)
        {
            Quaternion effectRotation = Quaternion.identity;

            if (collision.contacts.Length > 0)
            {
                effectRotation = Quaternion.LookRotation(
                    collision.contacts[0].normal
                );
            }

           // GameObject effect = ObjectPoolManager.Instance.Spawn(
                //effectPrefab,
                //transform.position,
                //effectRotation
           // );
            Destroy(Instantiate(effectPrefab,
                transform.position,
                effectRotation),5f);
            // 特效预制体需要有自己的计时脚本，到时调用Release
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyScript enemyScript =
                collision.gameObject.GetComponent<EnemyScript>();

            Enemybase enemyBase =
                collision.gameObject.GetComponentInParent<Enemybase>();

            if (enemyScript != null)
            {
                enemyScript.hit(damage);
            }
            else if (enemyBase != null)
            {
                enemyBase.hit(damage);
            }

            if (Player == null)
            {
                Player = GameObject.FindGameObjectWithTag("Player");
            }

            if (Player != null)
            {
                PlayerControlScript playerControl =
                    Player.GetComponent<PlayerControlScript>();

                if (playerControl != null)
                {
                    playerControl.Showcorsshiar();
                }
            }
        }

        pooledObject.Release();
    }
}