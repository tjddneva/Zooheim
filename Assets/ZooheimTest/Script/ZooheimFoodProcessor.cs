using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZooheimFoodProcessor : MonoBehaviour {
    public ZooheimMeatScript Meat;

    public int range;

    public void CreateMeat(float x, float z, float Calorie) {
        var CreatedMeat = Instantiate(Meat, new Vector3(x, 0f, z), Quaternion.identity);
        CreatedMeat.transform.parent = transform;
        CreatedMeat.Calorie = Calorie;
    }
}
