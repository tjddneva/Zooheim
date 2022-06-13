using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZooheimGrassScript : MonoBehaviour {
    public float Calorie;

    public void Awake() {
        Calorie = 15f;
    }

    public float Eaten() {
        Destroy(gameObject, 2f);
        return Calorie;
    }
}
