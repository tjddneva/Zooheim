using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class GiraffeAgent : BaseAnimalAgent {
    public override void AnimalInit() {
        AnimalHPLimit = 500f;

        AnimalMoveSpeed = 0.5f; //0.5f;
        AnimalTurnSpeed = 200f; //200f;
        AnimalSpeedLimit = 5f; //5f;
        AnimalAccelFactor = 1.5f;
        
        AnimalEnergyLimit = 450f;
        
        AnimalAttackPower = 30f;
        AnimalAttackEnergy = 5f;

        AnimalStarvationDamage = 0.02f;

        AnimalRunningConsumeEnergy = 0.5f;
        AnimalNormalConsumeEnergy = 0.25f;

        AnimalEatReward = 5f;
        AnimalChildbirthReward = 10f;

        AnimalDeathPenalty = -50f;
        AnimalStarvationPenalty = -0.01f;

        AnimalChildbirthPeriod = 40f;

        base.AnimalInit();
    }

    public override void OnTriggerEnter(Collider other) {
        if(other.gameObject.CompareTag("Grass") && AnimalEnergy < AnimalEnoughEnergy) {
            float AteCalorie = other.gameObject.GetComponent<Grass>().Eat();
            Eat(AteCalorie);
            Debug.Log("Tree collison");
            Freeze(2.0f);
        }
        else if(other.gameObject.CompareTag("Tree")) {
            float AteCalorie = other.gameObject.GetComponent<Wood>().Eat();
            Eat(AteCalorie);
            Freeze(2.0f);
        }
    }
}
