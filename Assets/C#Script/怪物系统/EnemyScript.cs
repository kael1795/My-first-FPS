using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum Enemytype
{
    Spider,     // 蜘蛛机器人
    Zombie      //普通僵尸
}
public class EnemyScript : MonoBehaviour
{

    //价值
    public int value = 10;
    private GameObject Player;
    //攻击伤害
    public float attackdamage = 10f;
    //攻速
    public float attackSpeed = 1f;
    //计时器
    public float timer = 0;
    public float movespeed = 3f;
    public float heathcount;
    public float maxheathcount=1000f;
    private Animator ani;
    private GameObject tar;
    private Rigidbody rb;
    private Vector3 velocity;
    private float ruturnspeed=10f;
    public MonsterType monsterType;
    //血条
    public Image hpUI;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        heathcount = maxheathcount;
        rb = GetComponent<Rigidbody>();
        tar = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        move();
        SethpUi(heathcount, maxheathcount);
    }
   public virtual void hit(float harm)
    {
        heathcount -= harm;
        Debug.Log("受到伤害" + harm);
        if (heathcount<=0)
        {
            Die();
            
        }
    }
    public virtual void Atatck(GameObject target)
    {
        if (target == null) return;
        PlayerControlScript ps = target.GetComponent<PlayerControlScript>();
        ps.Hit(attackdamage);
    }
    void move()
    {
        Vector3 dir = tar.transform.position - transform.position;
        dir.y = 0;
        Quaternion tarbot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation,tarbot,Time.deltaTime*ruturnspeed);
        velocity = dir.normalized * movespeed;
        rb.velocity = velocity;
    }
    public virtual void Die()
    {
        if (heathcount <= 0)
        {
            PlayerControlScript PCS = Player.GetComponent<PlayerControlScript>();
            PCS.Money += value;
            Destroy(gameObject);
        }
    }
    private  void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag=="Player" && timer>=attackSpeed)
        {
            timer = 0;
            Atatck(collision.gameObject);
        }
    }
    public void SethpUi(float currnthp, float maxhp)
    {
        hpUI.fillAmount = currnthp / maxhp;
    }
}
