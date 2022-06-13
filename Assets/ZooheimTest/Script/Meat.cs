using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meat : Feed
{
    // Start is called before the first frame update
    float baseSize = 500;
    
    override public void Start()
    {
        base.Start();
        size = 1.0f;
    }

    public void MeatInit(float cur_calorie)
    {
        this.cur_calorie = cur_calorie;
        size = cur_calorie / baseSize;
        transform_.localScale *= size;
    }
    // Update is called once per frame
    override public void Update()
    {
        
    }
}
