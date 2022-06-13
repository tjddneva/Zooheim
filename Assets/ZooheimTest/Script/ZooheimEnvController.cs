using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZooheimEnvController : MonoBehaviour {
    public ZooheimFoodProcessor FoodProcessor;

    public Field FieldControl;
    public BaseAnimalAgent[] Animals;
    [SerializeField]
    public GameObject[] AnimalGroupList;

    public int range = 50; //50;
    [SerializeField]
    public int[] AnimalNum;

    float WorldTimer;
    float LearningTimer;

    public bool isLearning;

    public void Start() {
        int x, z;
        WorldTimer = 0;
        LearningTimer = 0;

        FieldControl.CreateWorld();

        for(int i = 0; i < Animals.Length; i++) {
            for(int j = 0; j < AnimalNum[i]; j++) {
                x = Random.Range(-range, range);
                z = Random.Range(-range, range); 
                Instantiate(Animals[i], new Vector3(x, 0.5f, z), Quaternion.identity).transform.parent = AnimalGroupList[i].transform;
            }
        }
    }

    public void FixedUpdate() {
        WorldTimer += Time.deltaTime;
        LearningTimer += Time.deltaTime;
        if(isLearning && LearningTimer > 60f) FinishEpisode();
    }

    void FinishEpisode() {
        for(int i = 0; i < AnimalGroupList.Length; i++) {
            for(int j = 0; j < AnimalGroupList[i].transform.childCount; j++) {
                BaseAnimalAgent temp = AnimalGroupList[i].transform.GetChild(j).gameObject.GetComponent<BaseAnimalAgent>();
                temp.EndEpisode();
            }
        }
        ResetScene();
        LearningTimer = 0f;
    }

    void ResetScene() {
        Debug.Log("Reset Scene");

        for(int i = 0; i < AnimalGroupList.Length; i++) {
            Transform[] AnimalChildList = AnimalGroupList[i].GetComponentsInChildren<Transform>();
            for(int j = 1; j < AnimalChildList.Length; j++) {
                Destroy(AnimalChildList[j].gameObject);
            }
        }

        Transform[] FoodChildList = FoodProcessor.GetComponentsInChildren<Transform>();
        for(int i = 1; i < FoodChildList.Length; i++) Destroy(FoodChildList[i].gameObject);

        FieldControl.ClearWorld();

        int x, z;
        for(int i = 0; i < Animals.Length; i++) {
            for(int j = 0; j < AnimalNum[i]; j++) {
                x = Random.Range(-range, range);
                z = Random.Range(-range, range); 
                Instantiate(Animals[i], new Vector3(x, 0.5f, z), Quaternion.identity).transform.parent = AnimalGroupList[i].transform;
            }
        }

        FieldControl.CreateWorld();
    }
}
