using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowRowUI : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI InfoText;
    private int weaponID;
    private ShopUI shopUI;
    public Button BuyButton;
   
    public float money;
    private void Awake()
    {
        BuyButton.onClick.AddListener(OnRowClicked);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Setup(WeaponData data, ShopUI owner)
    {
        weaponID = data.weaponID;
        shopUI = owner;
        NameText.text = data.weaponName;
    }
    public void Refreshstate(WeaponManager WMR)
    {
        string info;
        if (WMR.IsUnlocked(weaponID))
        {
            info = "<color=#88FF88>已拥有</color>";
            BuyButton.interactable = false;
        }
        else
        {
            int price = WMR.GetcurrentWeaponData(weaponID).price;
            bool canAfford = shopUI.Getmoney() >= price;

            info = canAfford
                ? "<color=#FFFFFF>" + price + " 金币</color>"
                : "<color=#FF6666>" + price + " 金币</color>";

            BuyButton.interactable = canAfford;
        }
        if (InfoText.text != info) InfoText.text = info;
    }
    private void OnRowClicked()
    {
        Debug.Log("点到按钮");
        if (shopUI != null) shopUI.OnBuyClicked(weaponID);
    }
}
