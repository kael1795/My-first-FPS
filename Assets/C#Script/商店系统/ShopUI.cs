using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    // public ShowRowUI ;
    [Header("会被开/关的面板")]
    public GameObject ShopPanel;          

    [Header("开关按键")]
    public KeyCode toggleKey = KeyCode.B;
    private PlayerControlScript pc;
    private bool isOpen = false;
    public float money;
    public TextMeshProUGUI MoneyText;
    public TextMeshProUGUI RefillButtonText;
    public GameObject Player;
    public Transform RowContainer;
    public ShowRowUI RowPrefab;
    private readonly List<ShowRowUI> rows = new List<ShowRowUI>();
    [Header("补弹价格")]
    public int refillCost = 100;
    public Button RefillButton;
    // Start is called before the first frame update
    private void Awake()
    {
        Player = GameObject.FindWithTag("Player");
        pc = Player.GetComponent<PlayerControlScript>();
    }
    void Start()
    {
        BuildRows();
        RefillButton.onClick.AddListener(OnRefillClicked);
        Refresh();
        SetShopOpen(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            SetShopOpen(!isOpen);

        // 只在开着的时候刷新列表
        if (isOpen) Refresh();
    }
    public float Getmoney()
    {
        PlayerControlScript pc = Player.GetComponent<PlayerControlScript>();
        return pc.Money;
    }
    private void BuildRows()
    {
        if (WeaponManager.instance == null) return;

        List<WeaponData> list = WeaponManager.instance.AllWeaponList;
        for (int i = 0; i < list.Count; i++)
        {
            ShowRowUI row = Instantiate(RowPrefab, RowContainer);
            row.Setup(list[i], this);
            rows.Add(row);
        }
    }
    public void Refresh()
    {
        WeaponManager mgr = WeaponManager.instance;
        if (mgr == null) return;
        float money = Getmoney();
        // 金钱
        string moneyStr = "金币：" + money;
        if (MoneyText != null && MoneyText.text != moneyStr)
            MoneyText.text = moneyStr;

        // 每一行的状态
        for (int i = 0; i < rows.Count; i++)
            rows[i].Refreshstate(mgr);

        // 补弹按钮
        string refillStr = "补充弹药  -" + refillCost;
        if (RefillButtonText != null && RefillButtonText.text != refillStr)
            RefillButtonText.text = refillStr;

        if (RefillButton != null)
            RefillButton.interactable = money >= refillCost;
    }
    public void OnBuyClicked(int weaponID)
    {
        if (WeaponManager.instance == null) return;

        WeaponManager.instance.TryBuyWeapon(weaponID);   // 够钱就扣钱+解锁，不够就什么都不做
        Refresh();
    }
    private void OnRefillClicked()
    {
        if (WeaponManager.instance == null) return;

        WeaponManager.instance.RefillAmmo(refillCost);   // 扣钱 + 补满弹药
        Refresh();
    }
    public void SetShopOpen(bool open)
    {
        isOpen = open;

        if (ShopPanel != null) ShopPanel.SetActive(open);

        if (open)
        {
            if (pc != null) pc.enabled = false;    

            Cursor.lockState = CursorLockMode.None;        
            Cursor.visible = true;                        
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;      
            Cursor.visible = false;

            if (pc != null) pc.enabled = true;     
        }
    }
}
