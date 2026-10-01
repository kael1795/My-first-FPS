using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "新武器", menuName = "武器系统/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;          // 武器名
    public int weaponID;               // 唯一编号
    public int price;                  // 购买价格
    public bool isUnlocked;            // 是否解锁
    public int MaxreserveAmmo;         // 最大备弹量
   // public int CurrentreserveAmmo;     //当前备弹量
    public int MaxmagAmmo;             //最大弹夹容量
   // public int CurrentmagAmmo;         //当前弹夹容量
    public float damage;               // 伤害
    public float fireRate;             // 射速
    //public float reloadTime;           // 换弹时间
    public float recoil;               //后坐力
    public GameObject weaponPrefab;    // 武器模型
    public GameObject hitEffect;       // 命中特效
    public AudioClip fireSound;        // 开火音效
    public AudioClip EmptySound;       //空仓音效
    public GameObject bulletpref;      // 子弹
    public GameObject fireEffect;      //开火特效
    public AudioClip reloadSound;      //换单音效
   // public GameObject Firepoint;        //开火点


}
