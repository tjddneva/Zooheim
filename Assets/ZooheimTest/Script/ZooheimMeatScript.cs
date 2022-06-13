using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZooheimMeatScript : MonoBehaviour {
    public float Calorie;
    
    public float Eaten() {
        Destroy(gameObject, 2f);
        return Calorie;
    }
}
