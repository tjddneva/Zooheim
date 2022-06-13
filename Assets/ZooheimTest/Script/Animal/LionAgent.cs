using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class LionAgent : BaseAnimalAgent {
    public override void AnimalInit() {
        AnimalHPLimit = 100f;

        AnimalMoveSpeed = 1f; //1.5f;
        AnimalTurnSpeed = 150f; //200f;
        AnimalSpeedLimit = 6f; //9f;
        AnimalAccelFactor = 1.5f;
        
        AnimalEnergyLimit = 300f;
        
        AnimalAttackPower = 50f;
        AnimalAttackEnergy = 5f;

        AnimalStarvationDamage = 0.02f;

        AnimalRunningConsumeEnergy = 0.5f;
        AnimalNormalConsumeEnergy = 0.25f;

        AnimalEatReward = 5f;
        AnimalChildbirthReward = 10f;

        AnimalDeathPenalty = -50f;
        AnimalStarvationPenalty = -0.01f;

        AnimalChildbirthPeriod = 50f;

        base.AnimalInit();
    }

    public bool LionAttackValidCheck(Collision other) {
        if(other.gameObject.CompareTag("Deer")) return true;
        if(other.gameObject.CompareTag("DeerF")) return true;
        if(other.gameObject.CompareTag("Rabbit")) return true;
        if(other.gameObject.CompareTag("RabbitF")) return true;
        if(other.gameObject.CompareTag("Giraffe")) return true;
        if(other.gameObject.CompareTag("GiraffeF")) return true;
        if(other.gameObject.CompareTag("Wolf")) return true;
        if(other.gameObject.CompareTag("WolfF")) return true;
        return false;
    }

    public override void OnTriggerEnter(Collider other) {
        if(other.gameObject.CompareTag("Meat") && AnimalEnergy < AnimalEnoughEnergy && !AnimalDeadFlag) {
            float AteCalorie = other.gameObject.GetComponent<ZooheimMeatScript>().Eaten();
            Eat(AteCalorie);
            Freeze(3f);
        }
    }

    public override void OnCollisionEnter(Collision other) {
        if(LionAttackValidCheck(other)) {
            Attack(other.gameObject.GetComponent<BaseAnimalAgent>());
            Freeze(1.0f);
        }
    }

    public override void Attack(BaseAnimalAgent Enemy) {
        SwitchAttacking();
        float GivenDamage = AnimalAttackPower;
        float TakenDamage = Enemy.Resist(GivenDamage);
        //Freeze(1.0f);
        LoseHP(TakenDamage);
        AddReward((GivenDamage - TakenDamage) * 0.5f);
    }
}