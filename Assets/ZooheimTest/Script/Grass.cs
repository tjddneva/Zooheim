using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : Feed
{

    public GameObject GrassParent;
    public GameObject GrassPrefab1;
    public GameObject GrassPrefab2;

    public float reproduct_cycle = 60.0f;
    

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();

        max_calorie = 300;
        cur_calorie = 200;
        size = 1;
        ate_degree = 3;
        growth_speed = 0.01f;
        GrassParent = GameObject.Find("GrassParent");
    }

    // Update is called once per frame
    override public void Update()
    {
        transform_.localScale = new Vector3(5.0f, 5.0f, 5.0f) * size;
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
            
            if (Random.Range(0.0f, 1.0f) < 0.7f * Environment.GrassMap[x, z])
            {
                float dx = Random.Range(-0.5f, 0.5f) + transform_.position.x;
                float dz = Random.Range(-0.5f, 0.5f) + transform_.position.z;

                if (!(dx < Environment.width/2 * -1 || dx > Environment.width || dz < Environment.height/2 * -1 || dz < Environment.height / 2))
                {
                    GameObject curPrefab;

                    if (Random.Range(0.0f, 1.0f) > 0.5f) curPrefab = GrassPrefab1;
                    else curPrefab = GrassPrefab2;

                    GameObject newGrass = Instantiate(curPrefab,
                                                      new Vector3(transform_.position.x + dx, transform_.position.y, transform_.position.z + dz),
                                                      transform.rotation, GrassParent.transform);

                }

            }
        }

        if (cur_calorie <= 0)
        {
            //Destroy(this.gameObject);
        }

        // growth of grass
        if (max_calorie > cur_calorie)
        {
            cur_calorie += growth_speed;
            size = cur_calorie / max_calorie;
        }
    }
}
