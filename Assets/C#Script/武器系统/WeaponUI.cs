using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponUI : MonoBehaviour
{
    public TextMeshProUGUI ListText;
    public List<WeaponData> AllWeaponList;
    public int currentWeaponIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public  void Refersh()
    {
        AllWeaponList = WeaponManager.instance.AllWeaponList;
        currentWeaponIndex = WeaponManager.instance.currentWeaponIndex;
        if (ListText == null) return;

        string result = "";
        for (int i=0;i<AllWeaponList.Count ; i++)
        {
            if (AllWeaponList[i].isUnlocked)
            {
                result += (i == currentWeaponIndex) ? "▶ " : "<color=#00000000>▶ </color>";
                result += AllWeaponList[i].weaponID+" ";
                result += AllWeaponList[i].weaponName;
                result += "\n";
            }else
            {
                result += "<color=#FF0000>✖ </color>";
                result += AllWeaponList[i].weaponID+" ";
                result += AllWeaponList[i].weaponName;
                result += "\n";
            }
        }
        ListText.text = result;
    }
}
