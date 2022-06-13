using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood : Feed
{
    public float max_size;

    public GameObject TreePrefab;
    public GameObject DeadTreePrefab;
    public GameObject WoodParent;

    public float reproduct_cycle = 180.0f;

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();

        max_calorie = 700;
        cur_calorie = 300;
        size = 1.0f;
        max_size = 2.0f;

        ate_degree = 1;
        growth_speed = 0.02f;

        if (Environment.isDay) onDayChangeGrowthSpeed();
        else onNightChangeGrowthSpeed();

        WoodParent = GameObject.Find("WoodParent");
    }

    public void WoodInit(float cur_calorie, float size)
    {
        this.cur_calorie = cur_calorie;
    }

    // Update is called once per frame
    override public void Update()
    {
        transform_.localScale = new Vector3(1.0f, 1.0f, 1.0f) * size;
    }

    public void onDayChangeGrowthSpeed()
    {
        growth_speed = 0.02f;
    }

    public void onNightChangeGrowthSpeed()
    {
        growth_speed = 0.01f;
    }

    private void FixedUpdate()
    {
        this.time += Time.deltaTime;

        if (time > reproduct_cycle)
        {
            time = 0.0f;
            // reproductive
            int x = (int)(transform_.position.x + Environment.width / 2);
            int z = (int)(transform_.position.z + Environment.height / 2);
            
            if (Random.Range(0.0f, 1.0f) < 0.4f * Environment.ForestMap[x, z])
            {
                float dx = Random.Range(-0.5f, 0.5f) + transform_.position.x;
                float dz = Random.Range(-0.5f, 0.5f) + transform_.position.z;

                if (!(dx < Environment.width / 2 * -1 || dx > Environment.width || dz < Environment.height / 2 * -1 || dz < Environment.height / 2))
                {
                    GameObject newTree = Instantiate(TreePrefab, new Vector3(transform_.position.x + dx, transform_.position.y, transform_.position.z + dz), transform.rotation, WoodParent.transform);
                }

            }
        }

        if (cur_calorie <= 0)
        {
            //Destroy(this.gameObject);
        }

        // growth
        if (max_calorie > cur_calorie)
        {
            cur_calorie += growth_speed;
        }
        
        if (max_size > size)
        {
            size += growth_speed / max_calorie;
        }
    }
    
    override public float Eat()
    {
        if(transform_ != null) {
            GameObject newDeadTree = Instantiate(DeadTreePrefab, new Vector3(transform_.position.x, transform_.position.y, transform_.position.z), transform.rotation, WoodParent.transform);
            DeadWood newDeadTreeScript = newDeadTree.GetComponent<DeadWood>();
            newDeadTreeScript.DeadWoodInit(this.size, this.max_calorie);
        }
        float calorie = base.Eat();
        return calorie;
    }
}

