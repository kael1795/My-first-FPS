using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string EnemyName;   // 怪物名字
    public int EnemyID;        // 怪物ID
    public GameObject prefab;  // 怪物预制体
    public float value;        // 怪物价值（击杀奖励）
    public int maxHp;          // 怪物最大血量
    public float speed;        // 怪物移动速度
    public int damage;         // 怪物攻击力
    public float attackRange;  // 怪物攻击范围
    public float attackCd;     // 怪物攻击冷却时间
    public GameObject deathEffect; // 死亡特效（可按类型配不同预制体）
}