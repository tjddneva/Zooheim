using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DeerAgent : BaseAnimalAgent {
    public override void AnimalInit() {
        AnimalHPLimit = 100f;

        AnimalMoveSpeed = 0.75f; //1f;
        AnimalTurnSpeed = 150f; //200f;
        AnimalSpeedLimit = 3.5f; //7f;
        AnimalAccelFactor = 1.5f;
        
        AnimalEnergyLimit = 200f;
        
        AnimalAttackPower = 10f;
        AnimalAttackEnergy = 5f;

        AnimalStarvationDamage = 0.02f;

        AnimalRunningConsumeEnergy = 0.5f;
        AnimalNormalConsumeEnergy = 0.25f;

        AnimalEatReward = 5f;
        AnimalChildbirthReward = 10f;

        AnimalDeathPenalty = -50f;
        AnimalStarvationPenalty = -0.01f;

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
