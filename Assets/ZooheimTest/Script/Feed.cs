using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feed : MonoBehaviour
{
    public float max_calorie = 300.0f;   // Maximum Calorie that object can get 
    public float cur_calorie = 300.0f;
    public float size = 1f;
    public float ate_degree = 3f;    // hardness of feed. how easy to eaten
    public float growth_speed;
    public float time = 0.0f;

    protected Transform transform_;
    protected Field Environment;

    virtual public void Start()
    {
        transform_ = this.GetComponent<Transform>();
        Environment = GameObject.Find("Map").GetComponent<Field>();
    }

    virtual public void Update()
    {
        
    }
    /*
    
    virtual public float Eat(float eat_power)
    {
        float consume = ate_degree * eat_power;
        cur_calorie -= consume;
        size = cur_calorie / max_calorie;

        return consume;
    }
    */
    virtual public float Eat()
    {
        Destroy(gameObject);
        return cur_calorie;
    }
}
