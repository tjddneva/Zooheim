using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BaseAnimalAgent : Agent {
    public ZooheimFoodProcessor FoodProcessor;
    //애니메이션 컴포넌트
    protected Animator AnimalAni;
    
    //물리엔진
    protected Rigidbody AnimalRigidBody;

    //현재 체력
    [SerializeField]
    protected float AnimalHP;
    //최대 체력
    protected float AnimalHPLimit;

    //이동 속도(Rigidbody에 지속적으로 가해주는 힘의 크기)
    protected float AnimalMoveSpeed;
    //회전 속도
    protected float AnimalTurnSpeed;
    //최대 이동 속도(velocity.sqrMagnitude로 계산되는 값)
    protected float AnimalSpeedLimit;
    //달릴 때 이동 속도에 곱해지는 값
    protected float AnimalAccelFactor;

    //현재 에너지
    [SerializeField]
    protected float AnimalEnergy;
    //최대 에너지
    protected float AnimalEnergyLimit;

    //공격력
    protected float AnimalAttackPower;
    //공격하는데 필요한 에너지의 양
    protected float AnimalAttackEnergy;
    
    //동물이 안전한 상태를 정하기 위한 지표(번식 등을 할 때 참고할 수 있도록 추가함)
    protected float AnimalEnoughHP;
    //동물이 배부른 상태를 정하기 위한 지표(번식 등을 할 때 참고할 수 있도록 추가함)
    protected float AnimalEnoughEnergy;

    //동물의 위험한 상태를 정하기 위한 지표
    protected float AnimalLeastHP;
    //동물의 굶주린 상태를 정하기 위한 지표(예를 들어 Energy < LeastEngery일때 ReduceHP(AnimalStarvationDamage) 같은 식으로 이용)
    protected float AnimalLeastEnergy;

    //동물이 굶주린 상태에서 지속적으로 받는 데미지
    protected float AnimalStarvationDamage;
    //동물이 지속적으로 소비하는 에너지
    protected float AnimalConsumeEnergy;
    //동물이 달릴 때 소비하는 에너지
    protected float AnimalRunningConsumeEnergy;
    //동물이 평소에 지속적으로 소비하는 에너지
    protected float AnimalNormalConsumeEnergy;

    //번식을 할 수 있는 주기
    [SerializeField]
    protected float AnimalChildbirthPeriod;

    //음식을 먹었을 때 상점
    protected float AnimalEatReward;
    //자식을 낳았을 때 상점
    protected float AnimalChildbirthReward;
    //죽었을 때 벌점
    protected float AnimalDeathPenalty;
    //굶고 있을 때 벌점
    protected float AnimalStarvationPenalty;
    
    //시간을 재기 위한 타이머
    protected float AnimalTimer;

    protected bool AnimalFreezeFlag;

    protected float range = 50f; //50f;

    protected bool AnimalDeadFlag;


    //생성될 때 실행되는 함수, 초기화에 이용
    protected virtual void Awake() {
        AnimalInit();
    }

    public virtual void AnimalInit() {
        AnimalRigidBody = GetComponent<Rigidbody>();
        AnimalAni = GetComponent<Animator>();
        FoodProcessor = FindObjectOfType<ZooheimFoodProcessor>();
        AnimalHP = AnimalHPLimit;
        AnimalEnergy = AnimalEnergyLimit;

        AnimalEnoughHP = AnimalHPLimit * 0.8f;
        AnimalEnoughEnergy = AnimalEnergyLimit * 0.8f;

        AnimalLeastHP = AnimalHPLimit * 0.2f;
        AnimalLeastEnergy = AnimalEnergyLimit * 0.2f;

        AnimalConsumeEnergy = AnimalNormalConsumeEnergy;

        AnimalTimer = 0f;
        AnimalFreezeFlag = false;
        AnimalDeadFlag = false;
        SwitchIdle();
    }
    
    //일정한 간격에 따라 호출됨
    public virtual void FixedUpdate() {
        AnimalTimer += Time.deltaTime;
        LoseEnergy(AnimalConsumeEnergy);
        if(AnimalEnergy < AnimalLeastEnergy) {
            LoseHP(AnimalStarvationDamage);
            AddReward(AnimalStarvationPenalty);
        }
        if(transform.localPosition.y < -5f) {
            AddReward(-10f);
            EndEpisode();
            Destroy(gameObject);
        }
    }

    //학습 episode가 시작될 때 호출됨
    public override void OnEpisodeBegin() {

    }

    //ml agent가 입력값을 주었을 때 실행됨
    public override void OnActionReceived(ActionBuffers actionBuffers) {
        MoveAgent(actionBuffers);
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(AnimalHP);
        sensor.AddObservation(AnimalEnergy);
    }

    //입력값을 수동으로 주었을 때 실행됨(Heuristic only 사용시)
    public override void Heuristic(in ActionBuffers actionsOut) {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;
        if (Input.GetKey(KeyCode.D)) discreteActionsOut[1] = 1;
        if (Input.GetKey(KeyCode.A)) discreteActionsOut[1] = 2;
        if (Input.GetKey(KeyCode.LeftShift)) discreteActionsOut[2] = 1;
    }

    //입력값을 기반으로 agent를 움직이기 위한 코드
    public virtual void MoveAgent(ActionBuffers actionBuffers) {
        if(AnimalFreezeFlag) return;
        var discreteActions = actionBuffers.DiscreteActions;
        
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forwardAxis = discreteActions[0];
        var rotateAxis = discreteActions[1];
        var Accel = discreteActions[2];

        float AnimalSpeed;
        if(Accel == 1) {
            AnimalConsumeEnergy = AnimalRunningConsumeEnergy;
            AnimalSpeed = AnimalMoveSpeed * AnimalAccelFactor;
            SwitchRunning();
        }
        else {
            AnimalSpeed = AnimalMoveSpeed;
            AnimalConsumeEnergy = AnimalNormalConsumeEnergy;
            SwitchWalking();
        }

        switch(forwardAxis) {
            case 0:
                SwitchIdle();
                break;
            case 1:
                dirToGo = transform.forward * AnimalSpeed;
                break;
        }

        switch(rotateAxis) {
            case 1:
                rotateDir = transform.up * 1f;
                break;
            case 2:
                rotateDir = transform.up * -1f;
                break;
        }

        transform.Rotate(rotateDir, Time.fixedDeltaTime * AnimalTurnSpeed);
        AnimalRigidBody.AddForce(dirToGo, ForceMode.VelocityChange);

        if (AnimalRigidBody.velocity.sqrMagnitude > AnimalSpeedLimit)
            AnimalRigidBody.velocity *= 0.95f;
    }

    //어떤 지역에 진입했을때 호출됨
    public virtual void OnTriggerEnter(Collider other) {
        //자식 클래스에서 상세 구현
    }

    //어떤 물체와 충돌했을때 호출됨
    public virtual void OnCollisionEnter(Collision other) {
        //자식 클래스에서 상세 구현
    }
    
    //어떤 물체와 계속 접촉하고 있을때 호출됨
    public virtual void OnCollisionStay(Collision other) {
        if(other.gameObject.CompareTag("Wall"))
            AddReward(-0.1f);
    }

    //먹는 기능을 구현한 함수
    public virtual void Eat(float Calorie) {
        SwitchEating();
        RecoverHP(Calorie * 0.1f);
        RecoverEnergy(Calorie);
        if(AnimalEnergy > AnimalEnoughEnergy) AddReward(Calorie * 0.1f);
        else if(AnimalEnergy < AnimalLeastEnergy) AddReward(Calorie * 0.5f);
        else AddReward(Calorie * 0.3f);      
    }

    //공격하는 기능을 구현한 함수
    public virtual void Attack(BaseAnimalAgent Enemy) {
        //자식 클래스에서 상세 구현
    }

    //공격받았을때 반격하는 기능을 구현한 함수
    public virtual float Resist(float TakenDamage) {
        LoseHP(TakenDamage);
        LoseEnergy(AnimalAttackEnergy);
        //SwitchAttacking();
        return AnimalAttackPower;
    }

    //자식을 낳는 기능을 구현한 함수
    public virtual void Childbirth() {
        //자식 클래스에서 상세 구현
    }

    //체력을 회복하는 함수
    public virtual void RecoverHP(float Recover) {
        AnimalHP += Recover;
        if(AnimalHP > AnimalHPLimit) AnimalHP = AnimalHPLimit;
    }

    //체력을 잃는 함수
    public virtual void LoseHP(float Damage) {
        AnimalHP -= Damage;
        if(AnimalHP < 0 && !AnimalDeadFlag) {
            AnimalFreezeFlag = true;
            AnimalDeadFlag = true;
            SwitchDead();
            Invoke("Death", 1);
        }
    }

    public virtual void RecoverEnergy(float Rest) {
        AnimalEnergy += Rest;
        if(AnimalEnergy > AnimalEnergyLimit) AnimalEnergy = AnimalEnergyLimit;
    }

    public virtual void LoseEnergy(float Loss) {
        AnimalEnergy -= Loss;
        if(AnimalEnergy < 0) AnimalEnergy = 0;
    }

    //객체가 사망했을때 실행되는 함수
    public virtual void Death() {
        AddReward(AnimalDeathPenalty);
        ProduceMeat();
        EndEpisode();
        Destroy(gameObject);
    }

    //객체가 굶주리고 있는지 확인하는 함수
    public void CheckStarvation() {
        if(AnimalEnergy < AnimalLeastEnergy) {
            LoseHP(AnimalStarvationDamage * Time.deltaTime);
            AddReward(AnimalStarvationPenalty * Time.deltaTime);
        }
    }

    //객체를 time 동안 멈추게 하는 함수
    public void Freeze(float time) {
        AnimalFreezeFlag = true;
        AnimalRigidBody.velocity = Vector3.zero;
        Invoke("UnFreeze", time);
    }

    public void UnFreeze() {
        AnimalFreezeFlag = false;
    }

    public void ProduceMeat() {
        FoodProcessor.CreateMeat(transform.localPosition.x, transform.localPosition.z, AnimalEnergy * 0.5f);
    }

    public void SwitchRunning() {
        foreach(var param in AnimalAni.parameters) {
            if(param.name == "isRunning") {
                AnimalAni.SetBool("isRunning", true);
            }
            else AnimalAni.SetBool(param.name, false);
        }
        // running은 모든 종에 탑재된 것으로 확인하여 예외처리 생략
    }

    //// 함수 수정 ////
    //animation flag state를 attack으로 변경
    public void SwitchAttacking() {
        bool expectedStateFound = false;
        foreach(var param in AnimalAni.parameters) {
            if(param.name == "isAttacking") {
                AnimalAni.SetBool("isAttacking", true);
                expectedStateFound = true;
            }
            else AnimalAni.SetBool(param.name, false);
        }

        if(expectedStateFound) return;
        else // attacking이 없으면 running을 대신 실행
            AnimalAni.SetBool("isRunning", true);
    }

    //// 함수 수정 ////
    //animation flag state를 walk으로 변경
    public void SwitchWalking() {
        bool expectedStateFound = false;
        foreach(var param in AnimalAni.parameters) {
            if(param.name == "isWalking") {
                AnimalAni.SetBool("isWalking", true);
                expectedStateFound = true;
            }
            else AnimalAni.SetBool(param.name, false);
        }

        if(expectedStateFound) return;
        else // walking이 없으면 running을 대신 실행 (rabbit 해당)
            AnimalAni.SetBool("isRunning", true);
    }


    //// 함수 수정 ////
    //animation flag state를 idle로 변경
    public void SwitchIdle() {
        // 모든 state flag를 false로 바꾸면 idle state로 전환됨
        foreach(var param in AnimalAni.parameters)
                AnimalAni.SetBool(param.name, false);
    }

     //// 함수 수정 ////
    //animation flag state를 death로 변경
    public void SwitchDead() {
        bool expectedStateFound = false;
        foreach(var param in AnimalAni.parameters) {
            if(param.name == "isDead") {
                AnimalAni.SetBool("isDead", true);
                expectedStateFound = true;
            }
            else AnimalAni.SetBool(param.name, false);
        }

        if(expectedStateFound) return;
        else // rabbit에는 isDead 대신 isDead_0, isDead_1이 존재하기 때문에 이를 고려
            AnimalAni.SetBool("isDead_0", true);
    }

     //// 함수 수정 ////
    //animation flag state를 eating으로 변경
    public void SwitchEating() {
        bool expectedStateFound = false;
        foreach(var param in AnimalAni.parameters) {
            if(param.name == "isEating") {
                AnimalAni.SetBool("isEating", true);
                expectedStateFound = true;
            }
            else AnimalAni.SetBool(param.name, false);
        }

        if(expectedStateFound) return;
        else // eating이 없으면 (모든 flag를 비활성화해서) idle을 대신 실행 (rabbit, lion, wolf 해당)
            foreach(var param in AnimalAni.parameters)
                AnimalAni.SetBool(param.name, false);
    }

}
