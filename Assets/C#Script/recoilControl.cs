using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class recoilControl : MonoBehaviour
{
    public float x =-3f;
    public float SpeedX = 10f;
    public float returSpeedX = 5f;
    public float targetrotdtion;
    public float currentRotation;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //恢复
        targetrotdtion = Mathf.Lerp(targetrotdtion,0,returSpeedX*Time.deltaTime);
        currentRotation = Mathf.Lerp(currentRotation, targetrotdtion, SpeedX * Time.deltaTime);
        transform.rotation = Quaternion.Euler(currentRotation,transform.localEulerAngles.y,0);
    }
    public void fire(float recoil)
    {
        targetrotdtion += -recoil;
    }
}
