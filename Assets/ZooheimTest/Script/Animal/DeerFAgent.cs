using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DeerFAgent : DeerAgent {
    public DeerAgent ChildPrefab;
    [SerializeField]
    protected bool AnimalChildbirthFlag;

    public override void CollectObservations(VectorSensor sensor) {
        base.CollectObservations(sensor);
        sensor.AddObservation(AnimalChildbirthFlag);
    }

    public override void FixedUpdate() {
        base.FixedUpdate();
        if((AnimalTimer > AnimalChildbirthPeriod) && !AnimalChildbirthFlag) {
            AnimalChildbirthFlag = true;
        }
    }
    public override void AnimalInit() {
        base.AnimalInit();
        AnimalChildbirthFlag = false;
        AnimalChildbirthPeriod = 30f;
    }

    public override void Childbirth() {
        Debug.Log("Deer Child birth");
        AnimalTimer = 0f;
        AnimalChildbirthFlag = false;
        float RandNum = Random.Range(0f, 1f);
        float RandomOffset = Random.Range(0.2f, 0.5f);
        Vector3 BirthPosition = new Vector3(RandomOffset, 0f, RandomOffset);
        BirthPosition += transform.localPosition;
        if(BirthPosition.x < -range) BirthPosition.x += 0.1f;
        if(BirthPosition.z < -range) BirthPosition.z += 0.1f;
        if(BirthPosition.x > range) BirthPosition.x -= 0.1f;
        if(BirthPosition.z > range) BirthPosition.z -= 0.1f;

        if(RandNum > 0.5f) {
            var Child = Instantiate(ChildPrefab, BirthPosition, Quaternion.identity);
            Child.GetComponent<DeerAgent>().AnimalInit();
            Child.transform.parent = transform.parent.transform;
        }
        else {
            var Child = Instantiate(gameObject, BirthPosition, Quaternion.identity);
            Child.GetComponent<DeerFAgent>().AnimalInit();
            Child.transform.parent = transform.parent.transform;
        }
            
    }

    public override void OnCollisionEnter(Collision other) {
        if(other.gameObject.CompareTag("Grass") && AnimalEnergy < AnimalEnoughEnergy) {
            float AteColorie = other.gameObject.GetComponent<Grass>().Eat();
            Eat(AteColorie);
            Freeze(2.0f);
        }
        else if(AnimalChildbirthFlag && other.gameObject.CompareTag("Deer")) {
            Childbirth();
            Freeze(2f);
            AddReward(AnimalChildbirthReward);
        }
    }
}
