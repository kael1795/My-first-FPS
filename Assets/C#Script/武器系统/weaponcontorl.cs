using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class weaponcontorl : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject firePoint;
    public GameObject bulletpref;
    public GameObject firePre;
    public float Bulletintervel=0.3f;
    private float timeer=0;
    private PlayerControlScript pc;
    private recoilControl rc;
    //开火声
    private AudioSource fireAudio;
    public AudioClip fireClip;
    //空仓声
    private AudioSource EmptyAudio;
    public AudioClip emptyclip;
    //弹匣设计
    //最大备弹量与当前备弹量
    public int maxAmmo=128;
    private int _cuttentreserveAmmo;
    public int cuttentreserveAmmo 
    {
        get => _cuttentreserveAmmo;
        set
        {
            if (_cuttentreserveAmmo != value)
            {
                _cuttentreserveAmmo = value;
                updateUI();
            }
        }
    }
    //最大弹匣容量与当前容量
    public int  maxMagAmmo=32;
    private int _cuttentAmmo;
    public int  cuttentAmmo
    {
        get => _cuttentAmmo;
            set
        {
            if (_cuttentAmmo!=value)
            {
                _cuttentAmmo = value;
                updateUI();
            }
        }
    }
    //换单速度
    public float reloadSpeed=1f;
    public AudioClip reloadcilp;
    public bool isreload=false;
    public Animator ani;
    public TMP_Text maga;
    //消耗弹药量
    public int consumeAmmo=0;
    //开火点
    public Transform firepoint;
    void Awake()
    {
        firepoint = transform.Find("firepoint");
    }
    void Start()
    {
        cuttentAmmo = maxMagAmmo;
        cuttentreserveAmmo = maxAmmo;
        pc = GetComponent<PlayerControlScript>();
        rc = GetComponent<recoilControl>();
        fireAudio = GetComponent<AudioSource>();
        EmptyAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
        timeer += Time.deltaTime;
        if (Input.GetMouseButton(0) && timeer>=Bulletintervel && isreload==false)
        {
            if (cuttentAmmo > 0)
            {
                cuttentAmmo--;
                timeer = 0;
               // rc.fire();
                fireAudio.PlayOneShot(fireClip, 0.8f);
                Instantiate(bulletpref, firePoint.transform.position, firePoint.transform.rotation);
                GameObject go = Instantiate(firePre, firePoint.transform.position, firePoint.transform.rotation);
                Destroy(go, 0.1f);
            }
        }
        if (Input.GetMouseButtonDown(0) && timeer >= Bulletintervel && cuttentAmmo ==0)
        {
            EmptyAudio.PlayOneShot(emptyclip);
        }
        if (Input.GetKeyDown(KeyCode.R) && cuttentAmmo < maxMagAmmo && cuttentreserveAmmo > 0)
        {
            if (isreload) return;
            isreload = true;
            ani.SetBool("Isreload", true);
            fireAudio.PlayOneShot(reloadcilp);
            Invoke("reload",reloadSpeed);
        }
       
    }
    void reload()
    {
        consumeAmmo = maxMagAmmo - cuttentAmmo;
        
        if (cuttentreserveAmmo < maxMagAmmo)
        {
            cuttentAmmo += cuttentreserveAmmo;
            cuttentreserveAmmo = 0;
            isreload = false;
            ani.SetBool("Isreload", false);
            return;
        }
        cuttentreserveAmmo -= consumeAmmo;
        cuttentAmmo += consumeAmmo;
        isreload = false;
        ani.SetBool("Isreload", false);
    }
    void updateUI()
    {
        maga.text = cuttentAmmo + "/" + cuttentreserveAmmo;
    }

}
