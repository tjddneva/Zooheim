using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadWood : MonoBehaviour
{
    // Start is called before the first frame update
    float time = 0;
    float growth_time = 10;
    float size;
    public float max_calorie;
    public float cur_calorie;
    float growth_speed = 0.03f;
    
    private Transform transform_;

    public GameObject WoodPrefab;
    public GameObject WoodParent;
    void Start()
    {
        transform_ = this.GetComponent<Transform>();
        WoodParent = GameObject.Find("WoodParent");
        cur_calorie = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (cur_calorie >= 300)
        {
            GameObject newWood = Instantiate(WoodPrefab, new Vector3(transform_.position.x, transform_.position.y, transform_.position.z), transform_.rotation, WoodParent.transform);
            Wood newWoodScript = newWood.GetComponent<Wood>();
            newWoodScript.WoodInit(cur_calorie, size);
            Destroy(this.gameObject);
        }
    }

    public void DeadWoodInit(float size, float max_calorie)
    {
        this.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f) * size;
        this.max_calorie = max_calorie;
    }

    private void FixedUpdate()
    {
        cur_calorie += growth_speed / max_calorie;
    }
}
