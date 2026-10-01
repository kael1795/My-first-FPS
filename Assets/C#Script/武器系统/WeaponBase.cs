using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WeaponBase : MonoBehaviour
{
    // Start is called before the first frame update
   
    public Camera mainCam;
    private float timeer = 0;
    private PlayerControlScript pc;
    private recoilControl rc;
    //开火特效
    public GameObject firePre;
    //子弹
    public GameObject bulletpref;
    //射速
    public float fireRate;
    //伤害
    public float damage;
    //后坐力
    public float recoil;
    //开火声
    public AudioClip fireSound;
    private AudioSource fireAudio;
    //空仓声
    private AudioSource EmptyAudio;
    public AudioClip EmptySound;
    //弹匣设计
    //最大备弹量与当前备弹量
    public int maxAmmo = 128;
    private int _currentreserveAmmo;
    public int currentreserveAmmo
    {
        get => _currentreserveAmmo;
        set
        {
            if (_currentreserveAmmo != value)
            {
                _currentreserveAmmo = value;
                updateUI();
            }
        }
    }
    //最大弹匣容量与当前容量
    public int maxMagAmmo = 32;
    private int _currentAmmo;
    public int currentAmmo
    {
        get => _currentAmmo;
        set
        {
            if (_currentAmmo != value)
            {
                _currentAmmo = value;
                updateUI();
            }
        }
    }
    //换单速度
    public float reloadSpeed = 3.2f;
    public AudioClip reloadcilp;
    public bool isreload = false;

    
    public Animator ani;
    public TMP_Text maga;

    //消耗弹药量
    public int consumeAmmo = 0;
    //开火点
    public Transform firepoint;
    void Awake()
    {
        firepoint = transform.Find("firepoint");
        PlayerControlScript player = transform.root.GetComponent<PlayerControlScript>();
        if (player == null)
        {
            Debug.LogError("找不到PlayerControlScript！", this);
            return;
        }
        maga = player.maga;
        if (maga == null)
        {
            Debug.LogError("Player身上的maga UI引用没有拖拽赋值！", this);
        }
        mainCam = Camera.main;
    }
    void Start()
    {
        maxAmmo = WeaponManager.instance.GetWeaponData().MaxreserveAmmo;
        maxMagAmmo = WeaponManager.instance.GetWeaponData().MaxmagAmmo;
        //currentAmmo = WeaponManager.instance.
        //currentreserveAmmo = maxAmmo;
        pc = transform.root.GetComponent<PlayerControlScript>();
        rc = WeaponManager.instance.rc;

        fireSound = WeaponManager.instance.GetWeaponData().fireSound;
        EmptySound = WeaponManager.instance.GetWeaponData().EmptySound;
        bulletpref = WeaponManager.instance.GetWeaponData().bulletpref;
        firePre = WeaponManager.instance.GetWeaponData().hitEffect;
        ani= GetComponentInParent<Animator>();
        reloadcilp = WeaponManager.instance.GetWeaponData().reloadSound;
        fireAudio = this.GetComponent<AudioSource>();
        EmptyAudio = this.GetComponent<AudioSource>();
        fireRate = WeaponManager.instance.GetWeaponData().fireRate;
        recoil = WeaponManager.instance.GetWeaponData().recoil;
        damage=WeaponManager.instance.GetWeaponData().damage;
    }

    // Update is called once per frame
    void Update()
    {
        timeer += Time.deltaTime;
        if (Input.GetMouseButton(0) && timeer >= fireRate && isreload == false)
        {
            if (currentAmmo > 0)
            {
                currentAmmo--;
                timeer = 0;
                rc.fire(recoil);
                fireAudio.PlayOneShot(fireSound, 0.8f);
                SpawnBullet();
               
                GameObject go = Instantiate(firePre, firepoint.transform.position, firepoint.transform.rotation);
                Destroy(go, 0.1f);
            }
        }
        if (Input.GetMouseButtonDown(0) && timeer >= fireRate && currentAmmo == 0)
        {
            EmptyAudio.PlayOneShot(EmptySound);
        }
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxMagAmmo && currentreserveAmmo > 0)
        {
            if (isreload) return;
            pc.SetReloading(true);
            isreload = true;
            ani.SetBool("Isreload", true);
            fireAudio.PlayOneShot(reloadcilp);
            Invoke("reload", reloadSpeed);
        }

        
    }
    void reload()
    {
        consumeAmmo = maxMagAmmo - currentAmmo;

        if (currentreserveAmmo < maxMagAmmo)
        {
            currentAmmo += currentreserveAmmo;
            currentreserveAmmo = 0;
            isreload = false;
            ani.SetBool("Isreload", false);
            return;
        }
        currentreserveAmmo -= consumeAmmo;
        currentAmmo += consumeAmmo;
        pc.SetReloading(false);
        isreload = false;
        ani.SetBool("Isreload", false);
    }
    void updateUI()
    {
        if (maga == null) return;
        maga.text = currentAmmo + "/" + currentreserveAmmo;
    }
    void SpawnBullet()
    {
        Ray ray = mainCam.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        float maxDistance = 1000f;
        Vector3 aimPoint = ray.GetPoint(maxDistance);

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            maxDistance
        ))
        {
            aimPoint = hit.point;
        }

        Vector3 bulletDir =
            (aimPoint - firepoint.position).normalized;

        GameObject bullet = ObjectPoolManager.Instance.Spawn(
            bulletpref,
            firepoint.position,
            Quaternion.LookRotation(bulletDir)
        );

        if (bullet == null)
        {
            Debug.LogError("子弹预制体没有设置或对象池生成失败。", this);
            return;
        }

        BulletControl bulletControl =
            bullet.GetComponentInChildren<BulletControl>();

        if (bulletControl == null)
        {
            Debug.LogError("子弹预制体上没有BulletControl。", bullet);

            PooledObject pooledObject =
                bullet.GetComponent<PooledObject>();

            if (pooledObject != null)
            {
                pooledObject.Release();
            }

            return;
        }

        bulletControl.damage = damage;
        
        bulletControl.SetDirection(bulletDir);
    }
}
