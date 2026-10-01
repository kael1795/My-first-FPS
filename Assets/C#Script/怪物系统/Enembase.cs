using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum MonsterType
{
    Spider,
    Zombie
}

public enum ZombieState
{
    Idle,
    Chase,
    Attack,
    Dead
}

public class Enemybase : MonoBehaviour, IPoolable
{
    [Header("数据配置")]
    public EnemyData data;
    public MonsterType monsterType;

    [Header("运行时状态")]
    public float heathcount;
    public float Timer;
    public float viewDistance = 10f;
    public float attackCooldown = 1f;
    [Header("对象池重置")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;

    private bool initialized;

    [SerializeField] private ZombieState currentState = ZombieState.Idle;
    private bool isDead;

    [Header("组件引用")]
    public NavMeshAgent agent;
    public Image hpUI;

    [SerializeField] private Transform player;
    [SerializeField] private PlayerControlScript playerControl;
    private Camera mainCamera;
    private bool hasWarnedPlayerControl;

    public float attackRange => data != null ? data.attackRange : 0f;
    public float AttackDamage => data != null ? data.damage : 0f;
    public float MoveSpeed => data != null ? data.speed : 0f;
    public int KillValue => data != null ? (int)data.value : 0;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        mainCamera = Camera.main;
    }

    private void Start()
    {
        if (!initialized)
        {
            ResetForSpawn();
            initialized = true;
        }
    }
    private void ResetForSpawn()
    {
        if (data == null)
        {
            Debug.LogError(
                $"{name} 没有配置 EnemyData，脚本已禁用。",
                this
            );

            enabled = false;
            return;
        }

        enabled = true;

        // UI和生命值
        heathcount = Mathf.Max(0f, data.maxHp);
        SethpUi(heathcount, data.maxHp);

        if (hpUI != null)
        {
            hpUI.gameObject.SetActive(true);
        }

        // AI状态
        currentState = ZombieState.Idle;
        isDead = false;
        Timer = 0f;
        hasWarnedPlayerControl = false;

        // 玩家引用
        player = null;
        playerControl = null;

        // 动画
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        // 物理
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 导航
        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = MoveSpeed;
            agent.updatePosition = true;
            agent.updateRotation = true;

            if (!agent.isOnNavMesh)
            {
                bool warped = agent.Warp(transform.position);

                if (!warped)
                {
                    Debug.LogWarning(
                        $"{name} 生成点距离NavMesh过远，NavMeshAgent无法启用。",
                        this
                    );
                }
            }

            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                agent.isStopped = true;
            }
        }

        // 重新查找玩家
        TryFindPlayer();
    }
    private void Update()
    {
        if (isDead)
        {
            return;
        }
        if (player != null && agent != null)
        {
            //Debug.Log(
              //  $"状态：{currentState}，" +
                //$"距离：{Vector3.Distance(transform.position, player.position):F2}，" +
                //$"视野：{viewDistance}，" +
                //$"Agent速度：{agent.speed}，" +
                //$"OnNavMesh：{agent.isOnNavMesh}"
            //);
        }
        // 即使玩家被销毁，已经进入死亡状态的敌人也可以正常死亡。
        if (currentState == ZombieState.Dead)
        {
            Die();
            return;
        }

        // 玩家可能晚于敌人生成，因此找不到时持续尝试查找。
        if (player == null && !TryFindPlayer())
        {
            return;
        }

        if (data == null)
        {
            return;
        }

        Timer += Time.deltaTime;
        SethpUi(heathcount, data.maxHp);
        StateMachine();
    }

    private bool TryFindPlayer()
    {
        GameObject playerObject;

        try
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
        }
        catch (UnityException)
        {
            Debug.LogError("场景中没有定义 Player 标签，Enemybase 已禁用。", this);
            enabled = false;
            return false;
        }

        if (playerObject == null)
        {
            player = null;
            playerControl = null;
            return false;
        }

        player = playerObject.transform;
        playerControl = player.GetComponent<PlayerControlScript>();
        return true;
    }

    private void StateMachine()
    {
        switch (currentState)
        {
            case ZombieState.Idle:
                IdleState();
                break;

            case ZombieState.Chase:
                ChaseState();
                break;

            case ZombieState.Attack:
                AttackState();
                break;

            case ZombieState.Dead:
                Die();
                break;
        }
    }

    private void IdleState()
    {
        SetAgentStopped(true);

        if (player == null)
        {
            return;
        }

        float view = Mathf.Max(0f, viewDistance);

        if (SqrDistanceToPlayer() <= view * view)
        {
            currentState = ZombieState.Chase;
        }
    }

    private void ChaseState()
    {
        if (player == null)
        {
            currentState = ZombieState.Idle;
            return;
        }

        SetAgentStopped(false);

        if (IsAgentReady())
        {
            agent.SetDestination(player.position);
        }

        float distanceSqr = SqrDistanceToPlayer();
        float range = Mathf.Max(0f, attackRange);
        float view = Mathf.Max(0f, viewDistance);

        if (distanceSqr <= range * range)
        {
            SetAgentStopped(true);
            currentState = ZombieState.Attack;
        }
        else if (distanceSqr > view * view)
        {
            SetAgentStopped(true);
            currentState = ZombieState.Idle;
        }
    }

    private void AttackState()
    {
        if (player == null)
        {
            currentState = ZombieState.Idle;
            return;
        }

        Vector3 lookTarget = player.position;
        lookTarget.y = transform.position.y;
        transform.LookAt(lookTarget);

        float range = Mathf.Max(0f, attackRange);

        if (SqrDistanceToPlayer() > range * range)
        {
            SetAgentStopped(false);
            currentState = ZombieState.Chase;
            return;
        }

        float cooldown = Mathf.Max(0.01f, attackCooldown);

        if (Timer >= cooldown)
        {
            AttackPlayer();
            Timer = 0f;
        }
    }

    private void AttackPlayer()
    {
        if (playerControl == null && player != null)
        {
            playerControl = player.GetComponent<PlayerControlScript>();
        }

        if (playerControl == null)
        {
            if (!hasWarnedPlayerControl)
            {
                hasWarnedPlayerControl = true;
                Debug.LogWarning($"{name} 找不到 PlayerControlScript，无法攻击玩家。", this);
            }

            return;
        }

        playerControl.Hit(AttackDamage);
    }

    public virtual void hit(float harm)
    {
        if (isDead || data == null || harm <= 0f)
        {
            return;
        }

        heathcount -= harm;
        Debug.Log($"{data.EnemyName} 受到 {harm} 伤害，剩余血量 {heathcount}");

        if (heathcount <= 0f)
        {
            heathcount = 0f;
            currentState = ZombieState.Dead;
            Die();
        }
    }
    public virtual void OnSpawn()
    {
        ResetForSpawn();
        initialized = true;
    }

    public virtual void OnDespawn()
    {
        initialized = true;

        // 防止归还后还执行Update中的逻辑
        isDead = true;
        currentState = ZombieState.Dead;

        // 清空计时和引用
        Timer = 0f;
        player = null;
        playerControl = null;

        // 停止协程和Invoke
        StopAllCoroutines();
        CancelInvoke();

        // 停止导航
        if (agent != null)
        {
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }

            agent.enabled = false;
        }

        // 停止物理运动
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    public virtual void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        currentState = ZombieState.Dead;

        SetAgentStopped(true);

        if (agent != null)
        {
            agent.enabled = false;
        }

        if (playerControl == null && player != null)
        {
            playerControl = player.GetComponent<PlayerControlScript>();
        }

        if (playerControl != null)
        {
            playerControl.Money += KillValue;
        }

        switch (monsterType)
        {
            case MonsterType.Spider:
                OnSpiderDeath();
                break;

            case MonsterType.Zombie:
                OnZombieDeath();
                break;
        }

        if (data != null && data.deathEffect != null)
        {
            Instantiate(data.deathEffect, transform.position, transform.rotation);
        }

        PooledObject pooledObject =
    GetComponentInParent<PooledObject>();

        if (pooledObject != null)
        {
            pooledObject.Release();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnSpiderDeath()
    {
        Debug.Log("蜘蛛机器人自爆！");
    }

    protected virtual void OnZombieDeath()
    {
        Debug.Log("僵尸倒地死亡");
    }

    public void SethpUi(float currentHp, float maxHp)
    {
        if (hpUI == null)
        {
            return;
        }

        hpUI.fillAmount = maxHp > 0f
            ? Mathf.Clamp01(currentHp / maxHp)
            : 0f;
    }

    private void LateUpdate()
    {
        if (hpUI == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            hpUI.transform.forward = mainCamera.transform.forward;
        }
    }

    private float SqrDistanceToPlayer()
    {
        if (player == null)
        {
            return float.PositiveInfinity;
        }

        return (player.position - transform.position).sqrMagnitude;
    }

    private bool IsAgentReady()
    {
        return agent != null
            && agent.enabled
            && agent.isOnNavMesh;
    }

    private void SetAgentStopped(bool stopped)
    {
        if (IsAgentReady())
        {
            agent.isStopped = stopped;
        }
    }
}