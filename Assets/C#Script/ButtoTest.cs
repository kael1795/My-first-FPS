using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtoTest : MonoBehaviour
{
    // Start is called before the first frame update
    public Button Bu;
    public GameObject Player;

    void Start()
    {
        Bu.onClick.AddListener(onClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void onClick()
    {
        weaponcontorl wea = Player.GetComponent<weaponcontorl>();
        PlayerControlScript pcs = Player.GetComponent<PlayerControlScript>();
        wea.cuttentreserveAmmo += 30;
        pcs.Money -= 10;
    }
}
