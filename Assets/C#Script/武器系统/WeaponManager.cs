using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager instance;
    [Header("全部武器列表")]
    public List<WeaponData> AllWeaponList;
    public int currentWeaponIndex = 0;
    private Dictionary<int, WeaponAmmoState> weaponAmmoDict = new Dictionary<int, WeaponAmmoState>();
    private WeaponData currentWeaponData;
    private WeaponBase currentWeaponBase;
    private GameObject currentWeaponObj;
    public Transform weaponAttachPoint;
    public recoilControl rc;
    public WeaponUI WUI;
    public GameObject Player;
    public PlayerControlScript pc;
    // Start is called before the first frame update
    private void Awake()
    {
        rc = GetComponent<recoilControl>();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        pc = GetComponent<PlayerControlScript>();
        foreach (var weapon in AllWeaponList)
        {
            if (!weaponAmmoDict.ContainsKey(weapon.weaponID))
            {
                // 初始化：弹匣装满，备弹给满
                weaponAmmoDict.Add(weapon.weaponID, new WeaponAmmoState(weapon.MaxmagAmmo, weapon.MaxreserveAmmo));
            }
        }
        SwitchWeaponByID(0);
        if (WUI != null) WUI.Refersh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public WeaponData GetcurrentWeaponData(int id)
    {
        return AllWeaponList[id];
    }
    public WeaponData GetWeaponData()
    {
        return AllWeaponList[currentWeaponIndex];
    }
    public void SwitchWeaponByID(int id)
    {
        
        
        if (AllWeaponList == null || id < 0 || id >= AllWeaponList.Count)
        {
            Debug.Log("武器ID {id} 超出武器列表范围");
            return;
        }
        WeaponData targetweapon = GetcurrentWeaponData(id);
        if (targetweapon == null) return;
        if (targetweapon.isUnlocked == false) { Debug.Log("还未购买"); if (id > 0) id = id - 1; return; }
        if (currentWeaponData != null && currentWeaponBase != null)
{
    if (weaponAmmoDict.TryGetValue(currentWeaponData.weaponID, out var oldAmmo))
    {
        oldAmmo.currentMag = currentWeaponBase.currentAmmo;
        oldAmmo.currentReserve = currentWeaponBase.currentreserveAmmo;
    }
}
        if (currentWeaponObj != null)
        {
            Destroy(currentWeaponObj);
        }
       
       
        currentWeaponData = GetcurrentWeaponData(id);
        currentWeaponObj = Instantiate(currentWeaponData.weaponPrefab, weaponAttachPoint);
        currentWeaponBase = currentWeaponObj.GetComponent<WeaponBase>();
        currentWeaponIndex = id;
        if (weaponAmmoDict.TryGetValue(currentWeaponData.weaponID, out var NewAmmo))
        {
            currentWeaponBase.currentAmmo = NewAmmo.currentMag;
            currentWeaponBase.currentreserveAmmo = NewAmmo.currentReserve;
        }
        Debug.Log("切换为"+targetweapon.weaponName);
        if (WUI != null) WUI.Refersh();
    }
    public bool IsUnlocked(int id)
    {
        WeaponData weapon = GetcurrentWeaponData(id);
        if (weapon == null) return false;
        return weapon.isUnlocked;
    }
    public bool TryBuyWeapon(int weaponID)         
    {
        var w = GetcurrentWeaponData(weaponID);
        if (w == null || w.isUnlocked) return false;
        if (pc.Money < w.price) return false;

        pc.Money -= w.price;
        w.isUnlocked = true;
        WUI.Refersh();
        return true;
    }
    public void RefillAmmo(int cost) 
    { 
        
        if (pc.Money < cost) return;
        pc.Money -= cost;
        currentWeaponBase.currentreserveAmmo = currentWeaponBase.maxAmmo;
        
    }
}
