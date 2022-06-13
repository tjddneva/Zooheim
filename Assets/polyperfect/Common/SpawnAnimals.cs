using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
//using Vector3 = UnityEngine.Vector3;

public class SpawnAnimals : MonoBehaviour
{
    [SerializeField] GameObject[] characters;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(characters[0], new Vector3(-14.0f, 2.0f,30.0f) ,Quaternion.identity);
        Instantiate(characters[1], new Vector3(-14.0f, 2.0f, 40.0f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
