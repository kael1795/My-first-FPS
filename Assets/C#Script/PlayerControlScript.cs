using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerControlScript : MonoBehaviour
{
    private enum MovementState
    {
        Idle,
        Walking,
        Running
    }

    private enum ActionState
    {
        Normal,
        Reloading,
        Dead
    }

    private Rigidbody rb;
    private Animator ani;

    // 角色UI界面
    public Canvas playerUICanvas;
    public TMP_Text maga; // 弹药显示
    public GameObject mingzhongzhunxcing; // 准星
    public float hitMarkerDuration = 0.18f;

    private Coroutine hitMarkerCoroutine;
    private Vector3 hitMarkerBaseScale;

    // 资金
    private int _money = 0;
    public TMP_Text MoneyShow;

    // 速度
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float movespeed = 3f;

    // 灵敏度
    public float xScensitivity = 10f;
    public float yScensitivity = 10f;

    private float xRotation;
    private Vector3 velocity;

    public float Jumpforce = 5f;
    private bool jump;

    // 生命值
    public float heath = 100f;
    public float maxHeath = 100f;
    public Image hpUI;

    // 当前武器ID
    public int id = 0;

    // 移动状态机
    private MovementState movementState = MovementState.Idle;
    private bool sprintBlockedUntilShiftReleased;

    // 动作状态机
    private ActionState actionState = ActionState.Normal;

    public bool IsRunning => movementState == MovementState.Running;
    public bool IsReloading => actionState == ActionState.Reloading;
    public bool IsDead => actionState == ActionState.Dead;

    public int Money
    {
        get => _money;
        set
        {
            if (_money != value)
            {
                _money = value;
                UpdateMoney();
            }
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ani = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;

        if (mingzhongzhunxcing != null)
        {
            hitMarkerBaseScale = mingzhongzhunxcing.transform.localScale;
            mingzhongzhunxcing.SetActive(false);
        }

        ChangeMovementState(MovementState.Idle, true);
        ChangeActionState(ActionState.Normal);
    }

    void Update()
    {
        if (IsDead)
            return;

        Mouse();
        HandleFireWhileRunning();
        UpdateMovementState();
        Jump();
        move();
        Aim();
        SethpUi(heath, maxHeath);
        SwitchWeaponByID();
    }

    void Mouse()
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        xRotation -= y * yScensitivity;
        xRotation = Mathf.Clamp(xRotation, -80, 80);

        if (ani != null)
        {
            ani.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }

        transform.Rotate(Vector3.up * x * xScensitivity);
    }

    void move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 dir = (transform.forward * v + transform.right * h).normalized;

        velocity = dir * movespeed;
        velocity.y = rb.velocity.y;

        if (ani != null)
        {
            ani.SetFloat("Movement", dir.magnitude);
        }
    }

    void HandleFireWhileRunning()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CancelSprintForFire();
        }
    }

    // 如果开火逻辑在WeaponManager中，也可以在实际开火位置调用此方法
    public void CancelSprintForFire()
    {
        if (movementState != MovementState.Running)
            return;

        // 防止按住Shift时下一帧立刻恢复奔跑
        sprintBlockedUntilShiftReleased = Input.GetKey(KeyCode.LeftShift);
        ChangeMovementState(MovementState.Walking);
    }

    void UpdateMovementState()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float inputAmount = new Vector2(h, v).magnitude;
        bool sprintHeld = Input.GetKey(KeyCode.LeftShift);

        if (!sprintHeld)
        {
            sprintBlockedUntilShiftReleased = false;
        }

        if (inputAmount < 0.01f)
        {
            ChangeMovementState(MovementState.Idle);
        }
        else if (sprintHeld && !sprintBlockedUntilShiftReleased)
        {
            ChangeMovementState(MovementState.Running);
        }
        else
        {
            ChangeMovementState(MovementState.Walking);
        }
    }

    void ChangeMovementState(MovementState nextState, bool force = false)
    {
        if (!force && movementState == nextState)
            return;

        movementState = nextState;
        movespeed = nextState == MovementState.Running ? runSpeed : walkSpeed;

        if (ani != null)
        {
            ani.SetBool("Running", nextState == MovementState.Running);
        }
    }

    void ChangeActionState(ActionState nextState)
    {
        // 死亡后不允许切回普通或换弹状态
        if (actionState == ActionState.Dead && nextState != ActionState.Dead)
            return;

        actionState = nextState;
    }

    // WeaponManager开始换弹时传入true，结束或中断换弹时传入false
    public void SetReloading(bool reloading)
    {
        ChangeActionState(reloading ? ActionState.Reloading : ActionState.Normal);
    }

    void Aim()
    {
        if (ani == null)
            return;

        if (Input.GetMouseButton(1))
        {
            float aim = ani.GetFloat("Aiming");
            ani.SetFloat("Aiming", Mathf.Lerp(aim, 1, 0.1f));
        }
        else
        {
            float aim = ani.GetFloat("Aiming");
            ani.SetFloat("Aiming", Mathf.Lerp(aim, 0, 0.1f));
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            float aim = ani.GetFloat("Aiming");
            ani.SetFloat("Aiming", Mathf.Lerp(aim, 1, 0.1f));
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isground())
        {
            jump = true;
            velocity.y = Jumpforce;
        }
    }

    public void Hit(float Damage)
    {
        if (IsDead)
            return;

        heath -= Damage;

        if (heath <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        ChangeActionState(ActionState.Dead);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool isground()
    {
        RaycastHit hit;

        bool res = Physics.Raycast(
            transform.position + Vector3.up * 0.2f,
            -Vector3.up,
            out hit,
            0.4f,
            LayerMask.GetMask("Ground")
        );

        return res;
    }

    private void FixedUpdate()
    {
        if (IsDead || rb == null)
            return;

        if (jump)
        {
            jump = false;
            velocity.y = Jumpforce;
        }

        rb.velocity = velocity;
    }

    public void SethpUi(float currnthp, float maxhp)
    {
        if (hpUI == null || maxhp <= 0)
            return;

        hpUI.fillAmount = currnthp / maxhp;
    }

    void UpdateMoney()
    {
        if (MoneyShow != null)
        {
            MoneyShow.text = "$" + Money;
        }
    }

    void SwitchWeaponByID()
    {
        // 换弹、死亡时禁止切枪
        if (IsReloading || IsDead)
            return;

        int requestedId = -1;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            requestedId = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            requestedId = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            requestedId = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            requestedId = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            requestedId = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            requestedId = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha7))
            requestedId = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            requestedId = 7;

        if (requestedId < 0)
            return;

        id = requestedId;

        if (WeaponManager.instance != null)
        {
            WeaponManager.instance.SwitchWeaponByID(id);
        }
    }

    public void Showcorsshiar()
    {
        if (mingzhongzhunxcing == null)
            return;

        if (hitMarkerCoroutine != null)
        {
            StopCoroutine(hitMarkerCoroutine);
        }

        hitMarkerCoroutine = StartCoroutine(ShowHitMarkerRoutine());
    }

    private IEnumerator ShowHitMarkerRoutine()
    {
        mingzhongzhunxcing.SetActive(true);

        Transform marker = mingzhongzhunxcing.transform;
        CanvasGroup canvasGroup = mingzhongzhunxcing.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = mingzhongzhunxcing.AddComponent<CanvasGroup>();
        }

        marker.localScale = hitMarkerBaseScale;
        canvasGroup.alpha = 1f;

        float elapsed = 0f;

        while (elapsed < hitMarkerDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / hitMarkerDuration);

            marker.localScale = Vector3.Lerp(
                hitMarkerBaseScale * 0.8f,
                hitMarkerBaseScale,
                progress
            );

            canvasGroup.alpha = 1f - progress;

            yield return null;
        }

        marker.localScale = hitMarkerBaseScale;
        canvasGroup.alpha = 1f;
        mingzhongzhunxcing.SetActive(false);

        hitMarkerCoroutine = null;
    }
}