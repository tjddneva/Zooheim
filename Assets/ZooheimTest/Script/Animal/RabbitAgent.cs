using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RabbitAgent : BaseAnimalAgent {
    public override void AnimalInit() {
        AnimalHPLimit = 50f;

        AnimalMoveSpeed = 1.5f;
        AnimalTurnSpeed = 200f; //400f;
        AnimalSpeedLimit = 5f; //10f;
        AnimalAccelFactor = 1.5f; //2f;
        
        AnimalEnergyLimit = 150f;
        
        AnimalAttackPower = 5f;
        AnimalAttackEnergy = 5f;

        AnimalStarvationDamage = 0.01f;

        AnimalRunningConsumeEnergy = 0.5f;
        AnimalNormalConsumeEnergy = 0.25f;

        AnimalEatReward = 5f;
        AnimalChildbirthReward = 10f;

        AnimalDeathPenalty = -50f;
        AnimalStarvationPenalty = -0.01f;

        AnimalChildbirthPeriod = 25f;

        base.AnimalInit();
    }


    public override void OnTriggerEnter(Collider other) {
        if(other.gameObject.CompareTag("Grass") && AnimalEnergy < AnimalEnoughEnergy) {
            float AteColorie = other.gameObject.GetComponent<Grass>().Eat();
            Eat(AteColorie);
            Freeze(2.0f);
        }
    }
}
