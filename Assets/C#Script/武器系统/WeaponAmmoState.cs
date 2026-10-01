using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponAmmoState
{
    public int currentMag;     // 当前弹匣内子弹
    public int currentReserve; // 剩余备弹

    // 构造函数：初始化弹药
    public WeaponAmmoState(int magSize, int maxReserve)
    {
        currentMag = magSize;
        currentReserve = maxReserve;
    }
}