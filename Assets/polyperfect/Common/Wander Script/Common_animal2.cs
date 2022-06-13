using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Polyperfect.Common
{
    [RequireComponent(typeof(Animator)), RequireComponent(typeof(CharacterController))]
    public class Common_animal2 : MonoBehaviour
    {
        private const float contingencyDistance = 1f;

        [SerializeField] public IdleState[] idleStates;
        [SerializeField] private MovementState[] movementStates;
        [SerializeField] private AIState[] attackingStates;
        [SerializeField] private AIState[] deathStates;

        [SerializeField] public string species = "NA";

        [SerializeField, Tooltip("This specific animal stats asset, create a new one from the asset menu under (LowPolyAnimals/NewAnimalStats)")]
        public AIStats stats;

        [SerializeField, Tooltip("How far away from it's origin this animal will wander by itself.")]
        private float wanderZone = 10f;

        public float MaxDistance
        {
            get { return wanderZone; }
            set
            {
#if UNITY_EDITOR
                SceneView.RepaintAll();
#endif
                wanderZone = value;
            }
        }

        // [SerializeField, Tooltip("How dominent this animal is in the food chain, agressive animals will attack less dominant animals.")]
        private int dominance = 1;
        private int originalDominance = 0;

        [SerializeField, Tooltip("How far this animal can sense a predator.")]
        private float awareness = 30f;

        [SerializeField, Tooltip("How far this animal can sense it's prey.")]
        private float scent = 30f;

        private float originalScent = 0f;

        // [SerializeField, Tooltip("How many seconds this animal can run for before it gets tired.")]
        private float stamina = 10f;

        // [SerializeField, Tooltip("How much this damage this animal does to another animal.")]
        private float power = 10f;

        // [SerializeField, Tooltip("How much health this animal has.")]
        private float toughness = 5f;

        // [SerializeField, Tooltip("Chance of this animal attacking another animal."), Range(0f, 100f)]
        private float aggression = 0f;
        private float originalAggression = 0f;

        // [SerializeField, Tooltip("How quickly the animal does damage to another animal (every 'attackSpeed' seconds will cause 'power' amount of damage).")]
        private float attackSpeed = 0.5f;

        // [SerializeField, Tooltip("If true, this animal will attack other animals of the same specices.")]
        private bool territorial = false;

        // [SerializeField, Tooltip("Stealthy animals can't be detected by other animals.")]
        private bool stealthy = false;

        [SerializeField, Tooltip("If true, this animal will never leave it's zone, even if it's chasing or running away from another animal.")]
        private bool constainedToWanderZone = false;

        [SerializeField, Tooltip("This animal will be peaceful towards species in this list.")]
        private string[] nonAgressiveTowards;

        private static List<Common_animal2> allAnimals = new List<Common_animal2>();

        public static List<Common_animal2> AllAnimals
        {
            get { return allAnimals; }
        }

        //[Space(), Space(5)]
        [SerializeField, Tooltip("If true, this animal will rotate to match the terrain. Ensure you have set the layer of the terrain as 'Terrain'.")]
        private bool matchSurfaceRotation = false;

        [SerializeField, Tooltip("How fast the animnal rotates to match the surface rotation.")]
        private float surfaceRotationSpeed = 2f;

        //[Space(), Space(5)]
        [SerializeField, Tooltip("If true, AI changes to this animal will be logged in the console.")]
        private bool logChanges = false;

        [SerializeField, Tooltip("If true, gizmos will be drawn in the editor.")]
        private bool showGizmos = false;

        [SerializeField] private bool drawWanderRange = true;
        [SerializeField] private bool drawScentRange = true;
        [SerializeField] private bool drawAwarenessRange = true;

        public UnityEngine.Events.UnityEvent deathEvent;
        public UnityEngine.Events.UnityEvent attackingEvent;
        public UnityEngine.Events.UnityEvent idleEvent;
        public UnityEngine.Events.UnityEvent movementEvent;


        private Color distanceColor = new Color(0f, 0f, 205f);
        private Color awarnessColor = new Color(1f, 0f, 1f, 1f);
        private Color scentColor = new Color(1f, 0f, 0f, 1f);
        private Animator animator;
        private CharacterController characterController;
        private NavMeshAgent navMeshAgent;
        private Vector3 origin;

        private int totalIdleStateWeight;

        private bool useNavMesh = false;
        private Vector3 targetLocation = Vector3.zero;

        private float turnSpeed = 0f;

        public enum WanderState
        {
            Idle,
            Wander,
            Chase,
            Evade,
            Attack,
            Dead
        }

        float attackTimer = 0;
        float MinimumStaminaForAggression
        {
            get { return stats.stamina * .9f; }
        }

        float MinimumStaminaForFlee
        {
            get { return stats.stamina * .1f; }
        }

        public WanderState CurrentState;
        Common_animal2 primaryPrey;
        Common_animal2 primaryPursuer;
        Common_animal2 attackTarget;
        float moveSpeed = 0f;
        float attackReach = 2f;
        bool forceUpdate = false;
        float idleStateDuration;
        Vector3 startPosition;
        Vector3 wanderTarget;
        IdleState currentIdleState;
        float idleUpdateTime;

        // 생략
        public void OnDrawGizmosSelected()
        {
            if (!showGizmos)
                return;

            if (drawWanderRange)
            {
                // Draw circle of radius wander zone
                Gizmos.color = distanceColor;
                Gizmos.DrawWireSphere(origin == Vector3.zero ? transform.position : origin, wanderZone);

                Vector3 IconWander = new Vector3(transform.position.x, transform.position.y + wanderZone, transform.position.z);
                Gizmos.DrawIcon(IconWander, "ico-wander", true);
            }

            if (drawAwarenessRange)
            {
                //Draw circle radius for Awarness.
                Gizmos.color = awarnessColor;
                Gizmos.DrawWireSphere(transform.position, awareness);


                Vector3 IconAwareness = new Vector3(transform.position.x, transform.position.y + awareness, transform.position.z);
                Gizmos.DrawIcon(IconAwareness, "ico-awareness", true);
            }

            if (drawScentRange)
            {
                //Draw circle radius for Scent.
                Gizmos.color = scentColor;
                Gizmos.DrawWireSphere(transform.position, scent);

                Vector3 IconScent = new Vector3(transform.position.x, transform.position.y + scent, transform.position.z);
                Gizmos.DrawIcon(IconScent, "ico-scent", true);
            }

            if (!Application.isPlaying)
                return;

            // Draw target position.
            if (useNavMesh)
            {
                if (navMeshAgent.remainingDistance > 1f)
                {
                    Gizmos.DrawSphere(navMeshAgent.destination + new Vector3(0f, 0.1f, 0f), 0.2f);
                    Gizmos.DrawLine(transform.position, navMeshAgent.destination);
                }
            }
            else
            {
                if (targetLocation != Vector3.zero)
                {
                    Gizmos.DrawSphere(targetLocation + new Vector3(0f, 0.1f, 0f), 0.2f);
                    Gizmos.DrawLine(transform.position, targetLocation);
                }
            }
        }

        // animator component, runtimeController 생성하기
        private void Awake()
        {
            if (!stats)
            {
                Debug.LogError(string.Format("No stats attached to {0}'s Wander Script.", gameObject.name));
                enabled = false;
                return;
            }

            animator = GetComponent<Animator>();

            var runtimeController = animator.runtimeAnimatorController;
            if (animator)
                animatorParameters.UnionWith(animator.parameters.Select(p => p.name));

            if (logChanges)
            {
                if (runtimeController == null)
                {
                    Debug.LogError(string.Format(
                        "{0} has no animator controller, make sure you put one in to allow the character to walk. See documentation for more details (1)",
                        gameObject.name));
                    enabled = false;
                    return;
                }

                if (animator.avatar == null)
                {
                    Debug.LogError(string.Format("{0} has no avatar, make sure you put one in to allow the character to animate. See documentation for more details (2)",
                        gameObject.name));
                    enabled = false;
                    return;
                }

                if (animator.hasRootMotion == true)
                {
                    Debug.LogError(string.Format(
                        "{0} has root motion applied, consider turning this off as our script will deactivate this on play as we do not use it (3)", gameObject.name));
                    animator.applyRootMotion = false;
                }

                if (idleStates.Length == 0 || movementStates.Length == 0)
                {
                    Debug.LogError(string.Format("{0} has no idle or movement states, make sure you fill these out. See documentation for more details (4)",
                        gameObject.name));
                    enabled = false;
                    return;
                }

                if (idleStates.Length > 0)
                {
                    for (int i = 0; i < idleStates.Length; i++)
                    {
                        if (idleStates[i].animationBool == "")
                        {
                            Debug.LogError(string.Format(
                                "{0} has " + idleStates.Length +
                                " Idle states, you need to make sure that each state has an animation boolean. See documentation for more details (4)", gameObject.name));
                            enabled = false;
                            return;
                        }
                    }
                }

                if (movementStates.Length > 0)
                {
                    for (int i = 0; i < movementStates.Length; i++)
                    {
                        if (movementStates[i].animationBool == "")
                        {
                            Debug.LogError(string.Format(
                                "{0} has " + movementStates.Length +
                                " Movement states, you need to make sure that each state has an animation boolean to see the character walk. See documentation for more details (4)",
                                gameObject.name));
                            enabled = false;
                            return;
                        }

                        if (movementStates[i].moveSpeed <= 0)
                        {
                            Debug.LogError(string.Format(
                                "{0} has a movement state with a speed of 0 or less, you need to set the speed higher than 0 to see the character move. See documentation for more details (4)",
                                gameObject.name));
                            enabled = false;
                            return;
                        }

                        if (movementStates[i].turnSpeed <= 0)
                        {
                            Debug.LogError(string.Format(
                                "{0} has a turn speed state with a speed of 0 or less, you need to set the speed higher than 0 to see the character turn. See documentation for more details (4)",
                                gameObject.name));
                            enabled = false;
                            return;
                        }
                    }
                }

                if (attackingStates.Length == 0)
                {
                    Debug.Log(string.Format("{0} has " + attackingStates.Length + " this character will not be able to attack. See documentation for more details (4)",
                        gameObject.name));
                }

                if (attackingStates.Length > 0)
                {
                    for (int i = 0; i < attackingStates.Length; i++)
                    {
                        if (attackingStates[i].animationBool == "")
                        {
                            Debug.LogError(string.Format(
                                "{0} has " + attackingStates.Length +
                                " attacking states, you need to make sure that each state has an animation boolean. See documentation for more details (4)",
                                gameObject.name));
                            enabled = false;
                            return;
                        }
                    }
                }

                if (stats == null)
                {
                    Debug.LogError(string.Format("{0} has no AI stats, make sure you assign one to the wander script. See documentation for more details (5)",
                        gameObject.name));
                    enabled = false;
                    return;
                }

                if (animator)
                {
                    foreach (var item in AllStates)
                    {
                        if (!animatorParameters.Contains(item.animationBool))
                        {
                            Debug.LogError(string.Format(
                                "{0} did not contain {1}. Make sure you set it in the Animation States on the character, and have a matching parameter in the Animator Controller assigned.",
                                gameObject.name, item.animationBool));
                            enabled = false;
                            return;
                        }
                    }
                }
            }
            // 에러 메시지 검사 여기까지 //


            foreach (IdleState state in idleStates)
            {
                totalIdleStateWeight += state.stateWeight;
            }

            origin = transform.position;
            animator.applyRootMotion = false;
            characterController = GetComponent<CharacterController>();
            navMeshAgent = GetComponent<NavMeshAgent>();

            //Assign the stats to variables
            originalDominance = stats.dominance;
            dominance = originalDominance;

            toughness = stats.toughness;
            territorial = stats.territorial;

            stamina = stats.stamina;

            originalAggression = stats.agression;
            aggression = originalAggression;

            attackSpeed = stats.attackSpeed;
            stealthy = stats.stealthy;

            originalScent = scent;
            scent = originalScent;

            if (navMeshAgent)
            {
                useNavMesh = true;
                navMeshAgent.stoppingDistance = contingencyDistance;
            }

            if (matchSurfaceRotation && transform.childCount > 0)
            {
                transform.GetChild(0).gameObject.AddComponent<Common_SurfaceRotation>().SetRotationSpeed(surfaceRotationSpeed);
            }
        }

        IEnumerable<AIState> AllStates
        {
            get
            {
                foreach (var item in idleStates)
                    yield return item;
                foreach (var item in movementStates)
                    yield return item;
                foreach (var item in attackingStates)
                    yield return item;
                foreach (var item in deathStates)
                    yield return item;
            }
        }

        void OnEnable()
        {
            allAnimals.Add(this);
        }

        void OnDisable()
        {
            allAnimals.Remove(this);
            StopAllCoroutines();
        }

        // 내부 if문은 제외하고 구현할 것
        private void Start()
        {
            startPosition = transform.position;
            if (Common_WanderManager.Instance != null && Common_WanderManager.Instance.PeaceTime)
            {
                SetPeaceTime(true);
            }

            StartCoroutine(RandomStartingDelay());
        }

        bool started = false;
        readonly HashSet<string> animatorParameters = new HashSet<string>();

        void Update()
        {
            if (!started)
                return;
            if (forceUpdate) // 포식자가 자기를 공격해서 자기 상태가 강제로 바뀌었을 떄
            {
                UpdateAI();
                forceUpdate = false;
            }

            // Attack이 현재 state일 때
            if (CurrentState == WanderState.Attack)
            {
                // 공격대상이 없거나 죽었을 때
                if (!attackTarget || attackTarget.CurrentState == WanderState.Dead)
                {
                    var previous = attackTarget;
                    UpdateAI(); // 여기서 공격대상이 null이나 새 대상이 지정됨

                    // updateAI() 실행 후에도 공격대상이 바뀌지 않으면 에러임
                    if (previous && previous == attackTarget)
                        Debug.LogError(string.Format("Target was same {0}", previous.gameObject.name));
                }

                // 
                attackTimer += Time.deltaTime;
            }

            // attackTimer에 공격모션동안 걸리는 시간을 누적하다가 마지막 순간에 damage 적용시킴
            if (attackTimer > attackSpeed)
            {
                attackTimer -= attackSpeed; // damage 적용 후에는 공격누적시간 측정 변수를 (사실상) 초기화
                if (attackTarget)
                    attackTarget.TakeDamage(power);
                if (attackTarget.CurrentState == WanderState.Dead)
                    UpdateAI();
            }

            var position = transform.position;
            var targetPosition = position;
            switch (CurrentState)
            {
                case WanderState.Attack:
                    FaceDirection((attackTarget.transform.position - position).normalized); // 고개를 attackTarget으로 돌림
                    targetPosition = position;
                    break;

                case WanderState.Chase:
                    if (!primaryPrey || primaryPrey.CurrentState == WanderState.Dead) // 쫓는 대상이 null이 되거나, 죽으면  primaryPrey를 null로 만들고, 자기 상태는 Idle로 변경
                    {
                        primaryPrey = null;
                        SetState(WanderState.Idle);
                        goto case WanderState.Idle; // Idle 상태 코드부분으로 jump
                    }
                    // 계속 쫓고 있으면
                    targetPosition = primaryPrey.transform.position;
                    ValidatePosition(ref targetPosition);  // NavMesh만을 위한 함수

                    if (!IsValidLocation(targetPosition)) // target 위치가 valid한지 확인
                    {
                        SetState(WanderState.Idle);
                        targetPosition = position;
                        UpdateAI();
                        break;
                    }

                    FaceDirection((targetPosition - position).normalized); // 고개를 attackTarget으로 돌림
                    stamina -= Time.deltaTime; // 걸린 frame 시간만큼 stamina 감소시킴
                    if (stamina <= 0f)
                        UpdateAI();
                    break;

                case WanderState.Evade:
                    targetPosition = position + Vector3.ProjectOnPlane(position - primaryPursuer.transform.position, Vector3.up); // 도망갈 방향을 primaryPursuer의 반대로 설정
                    if (!IsValidLocation(targetPosition))
                        targetPosition = startPosition;
                    ValidatePosition(ref targetPosition); // NavMesh만을 위한 함수
                    FaceDirection((targetPosition - position).normalized); // 고개를 도망갈 방향으로 돌림
                    stamina -= Time.deltaTime;
                    if (stamina <= 0f)
                        UpdateAI();
                    break;

                case WanderState.Wander:
                    stamina = Mathf.MoveTowards(stamina, stats.stamina, Time.deltaTime); // stamina 충전
                    targetPosition = wanderTarget; // UpdateAI()->SetState()->Handle->HandleBeginWander() 에서 랜덤으로 생성
                    Debug.DrawLine(position, targetPosition, Color.yellow); // Gizmo 전용
                    FaceDirection((targetPosition - position).normalized); // 설정한 random targetPosition으로 고개 돌림
                    var displacementFromTarget = Vector3.ProjectOnPlane(targetPosition - transform.position, Vector3.up); // 경사 위에서 target과의 벡터계산
                    if (displacementFromTarget.magnitude < contingencyDistance) // random으로 정해진 targetPosition이 현위치랑 근접하면(< contingencyDistance), Idle로 전환
                    {
                        SetState(WanderState.Idle);
                        UpdateAI();
                    }

                    break;
                case WanderState.Idle:
                    stamina = Mathf.MoveTowards(stamina, stats.stamina, Time.deltaTime); // stamina 충전
                    if (Time.time >= idleUpdateTime) // idleUpdateTime은 HandleBeginIdle()에서 random으로 정해짐
                    {
                        SetState(WanderState.Wander);
                        UpdateAI();
                    }
                    break;
            }

            if (navMeshAgent)
            {
                navMeshAgent.destination = targetPosition;
                navMeshAgent.speed = moveSpeed;
                navMeshAgent.angularSpeed = turnSpeed;
            }
            else // navMash를 사용하지 않으니까 실질적으로 여기서 움직이게 됨.
                characterController.SimpleMove(moveSpeed * UnityEngine.Vector3.ProjectOnPlane(targetPosition - position, Vector3.up).normalized);


        }

        void FaceDirection(Vector3 facePosition)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.RotateTowards(transform.forward,
                facePosition, turnSpeed * Time.deltaTime * Mathf.Deg2Rad, 0f), Vector3.up), Vector3.up);
        }

        public void TakeDamage(float damage)
        {
            toughness -= damage;
            if (toughness <= 0f)
                Die();
        }
        public void Die()
        {
            SetState(WanderState.Dead);
        }

        public void SetPeaceTime(bool peace)
        {
            if (peace)
            {
                dominance = 0;
                scent = 0f;
                aggression = 0f;
            }
            else
            {
                dominance = originalDominance;
                scent = originalScent;
                aggression = originalAggression;
            }
        }

        void UpdateAI()
        {
            if (CurrentState == WanderState.Dead)
            {
                Debug.LogError("Trying to update the AI of a dead animal, something probably went wrong somewhere.");
                return;
            }

            var position = transform.position;
            primaryPursuer = null;

            // closestDistance: 나와 나랑 최단거리에있는포식자(primaryPursuer) 사이 거리, 모든 동물에 대해서 foreach로 확인
            if (awareness > 0)
            {
                var closestDistance = awareness;
                if (allAnimals.Count > 0)
                {
                    foreach (var chaser in allAnimals)
                    {
                        if (chaser.primaryPrey != this && chaser.attackTarget != this) // 
                            continue;

                        if (chaser.CurrentState == WanderState.Dead)
                            continue;
                        var distance = Vector3.Distance(position, chaser.transform.position);
                        if ((chaser.attackTarget != this && chaser.stealthy) || chaser.dominance <= this.dominance || distance > closestDistance)
                            continue;

                        closestDistance = distance;
                        primaryPursuer = chaser;
                    }
                }
            }

            var wasSameTarget = false;

            // primaryPrey가 이전 frame에 설정됐을 떄
            // 그것이 죽거나 내 탐지범위에서 벗어나면 primaryPrey = null이고,
            // 그렇지 않으면 그 target을 지금도 쫓고 있는 것.
            if (primaryPrey)
            {
                if (primaryPrey.CurrentState == WanderState.Dead)
                    primaryPrey = null;
                else
                {
                    var distanceToPrey = Vector3.Distance(position, primaryPrey.transform.position);
                    if (distanceToPrey > scent)
                        primaryPrey = null;
                    else
                        wasSameTarget = true;
                }
            }

            // primaryPrey가 없을 때 새로 설정시도하는 부분
            if (!primaryPrey)
            {
                primaryPrey = null;
                if (dominance > 0 && attackingStates.Length > 0)
                {
                    var aggFrac = aggression * .01f;
                    aggFrac *= aggFrac;
                    var closestDistance = scent;
                    foreach (var potentialPrey in allAnimals)
                    {
                        // 쫓아가려 했는데 그 애가 죽었을 때
                        if (potentialPrey.CurrentState == WanderState.Dead)
                            Debug.LogError(string.Format("Dead animal found: {0}", potentialPrey.gameObject.name));

                        // 쫓아갈 애가 나거나, 동족이면서 동족상잔을 하지않는 애거나, 나보다 우위에있거나, stealthy한애일 때
                        if (potentialPrey == this || (potentialPrey.species == species && !territorial) ||
                            potentialPrey.dominance > dominance || potentialPrey.stealthy)
                            continue;

                        // 미리 위에 설정해둔 nonAggressiveTowards에 해당할 때
                        if (nonAgressiveTowards.Contains(potentialPrey.species))
                            continue;

                        // (aggression(공격확률담당변수)*0.01)^2 값이 0~1사이의 랜덤값보다 작으면 공격 단념
                        if (Random.Range(0f, 0.99999f) >= aggFrac)
                            continue;

                        // 아래부터는 공격하는 케이스

                        var preyPosition = potentialPrey.transform.position;

                        // constainedWanderZone이 켜졌고 반경 밖으로 나갔을 때
                        if (!IsValidLocation(preyPosition))
                            continue;

                        // 나와 먹이와의 거리 갱신하고 primaryPrey로 간주
                        var distance = Vector3.Distance(position, preyPosition);
                        if (distance > closestDistance)
                            continue;
                        if (logChanges)
                            Debug.Log(string.Format("{0}: Found prey ({1}), chasing.", gameObject.name, potentialPrey.gameObject.name));
                        closestDistance = distance;
                        primaryPrey = potentialPrey;
                    }
                }
            }

            // true: primaryPrey를 공격함, false: primaryPrey를 공격하지 않음
            var aggressiveOption = false;

            // primaryPrey가 정해진 경우
            if (primaryPrey)
            {
                // primaryPrey가 이전frame과 동일하고 stamina가 남았을 떄거나, stamina가 aggression을 보일 정도로 충분할 때인지 확인
                // (이미 쫓던 애는 stamina가 양수면 계속 쫓고, 새로운 애를 쫓으려면 stamina가 minimumStamin 이상이어야 함)
                if ((wasSameTarget && stamina > 0) || stamina > MinimumStaminaForAggression)
                    aggressiveOption = true;
                else
                    primaryPrey = null;
            }

            var defensiveOption = false;

            // 쫓아오는 애가 있고, aggressiveOption과 primaryPrey가 false와 null일 떄
            if (primaryPursuer && !aggressiveOption)
            {
                // 도망갈 힘이 남았으면 defensiveOption을 켬
                if (stamina > MinimumStaminaForFlee)
                    defensiveOption = true;
            }

            var updateTargetAI = false;

            // agressiveOption이 켜졌고, 먹이와의거리가 공격가능거리 안이면 isPreyInAttackRange를 켬.
            var isPreyInAttackRange = aggressiveOption && Vector3.Distance(position, primaryPrey.transform.position) < CalcAttackRange(primaryPrey);
            // defensiveOption이 켜졌고, 포식자와의거리가 피해입는거리 안이면 isPursuerInAttackRange를 켬.
            var isPursuerInAttackRange = defensiveOption && Vector3.Distance(position, primaryPursuer.transform.position) < CalcAttackRange(primaryPursuer);

            // 포식자의 공격범위안에 자기가 들어오면
            if (isPursuerInAttackRange)
            {
                attackTarget = primaryPursuer; // 공격대상을 primaryPursuer로 설정
            }
            // 자기가 포식자의 공격범위 밖이고, 쫓던애가 자기의 공격범위에 들어서면
            else if (isPreyInAttackRange)
            {

                attackTarget = primaryPrey;
                // 자기가 쫓던애가 자기를 attackTarget으로 (역으로) 정하지 않았으면
                if (!attackTarget.attackTarget == this)
                    updateTargetAI = true;
            }
            // isPreyInAttackRange, isPursuerInAttackRange가 모두 false이면
            else
                attackTarget = null;

            // 상황에 따라 setState() 실행
            var shouldAttack = attackingStates.Length > 0 && (isPreyInAttackRange || isPursuerInAttackRange);
            if (shouldAttack)
                SetState(WanderState.Attack);
            else if (aggressiveOption)
                SetState(WanderState.Chase);
            else if (defensiveOption)
                SetState(WanderState.Evade);
            else if (CurrentState != WanderState.Idle && CurrentState != WanderState.Wander)
                SetState(WanderState.Idle);

            // 내가 공격을 해야되는 상황이고, 그 대상이 내 먹이일 때
            if (shouldAttack && updateTargetAI)
                attackTarget.forceUpdate = true; // 먹이의 Update()를 강제실행
        }

        bool IsValidLocation(Vector3 targetPosition)
        {
            if (!constainedToWanderZone)
                return true;
            var distanceFromWander = Vector3.Distance(startPosition, targetPosition);
            var isInWander = distanceFromWander < wanderZone;
            return isInWander;
        }

        // 공격할 만큼 충분히 가까이 있는지 확인하기 위해 공격가능거리를 반환
        float CalcAttackRange(Common_animal2 other)
        {
            var thisRange = navMeshAgent ? navMeshAgent.radius : characterController.radius;
            var thatRange = other.navMeshAgent ? other.navMeshAgent.radius : other.characterController.radius;
            return attackReach + thisRange + thatRange;
        }

        void SetState(WanderState state)
        {
            var previousState = CurrentState;
            if (previousState == WanderState.Dead)
            {
                Debug.LogError("Attempting to set a state to a dead animal.");
                return;
            }
            //if (state != previousState)
            {
                CurrentState = state;
                switch (CurrentState)
                {
                    case WanderState.Idle:
                        HandleBeginIdle();
                        break;
                    case WanderState.Chase:
                        HandleBeginChase();
                        break;
                    case WanderState.Evade:
                        HandleBeginEvade();
                        break;
                    case WanderState.Attack:
                        HandleBeginAttack();
                        break;
                    case WanderState.Dead:
                        HandleBeginDeath();
                        break;
                    case WanderState.Wander:
                        HandleBeginWander();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        void ClearAnimatorBools()
        {
            foreach (var item in idleStates)
                TrySetBool(item.animationBool, false);
            foreach (var item in movementStates)
                TrySetBool(item.animationBool, false);
            foreach (var item in attackingStates)
                TrySetBool(item.animationBool, false);
            foreach (var item in deathStates)
                TrySetBool(item.animationBool, false);
        }
        void TrySetBool(string parameterName, bool value)
        {
            if (!string.IsNullOrEmpty(parameterName))
            {
                if (logChanges || animatorParameters.Contains(parameterName))
                    animator.SetBool(parameterName, value);
            }
        }

        void HandleBeginDeath()
        {
            ClearAnimatorBools();
            if (deathStates.Length > 0)
                TrySetBool(deathStates[Random.Range(0, deathStates.Length)].animationBool, true);

            deathEvent.Invoke();
            if (navMeshAgent && navMeshAgent.isOnNavMesh)
                navMeshAgent.destination = transform.position;
            enabled = false;
        }

        void HandleBeginAttack()
        {
            var attackState = Random.Range(0, attackingStates.Length);
            turnSpeed = 120f;
            ClearAnimatorBools();
            TrySetBool(attackingStates[attackState].animationBool, true);
            attackingEvent.Invoke();
        }

        void HandleBeginEvade()
        {
            SetMoveFast();
            movementEvent.Invoke();
        }

        void HandleBeginChase()
        {
            SetMoveFast();
            movementEvent.Invoke();
        }

        void SetMoveFast()
        {
            MovementState moveState = null;
            var maxSpeed = 0f;
            foreach (var state in movementStates)
            {
                var stateSpeed = state.moveSpeed;
                if (stateSpeed > maxSpeed)
                {
                    moveState = state;
                    maxSpeed = stateSpeed;
                }
            }

            UnityEngine.Assertions.Assert.IsNotNull(moveState, string.Format("{0}'s wander script does not have any movement states.", gameObject.name));
            turnSpeed = moveState.turnSpeed;
            moveSpeed = maxSpeed;
            ClearAnimatorBools();
            TrySetBool(moveState.animationBool, true);
        }

        void SetMoveSlow()
        {
            MovementState moveState = null;
            var minSpeed = float.MaxValue;
            foreach (var state in movementStates)
            {
                var stateSpeed = state.moveSpeed;
                if (stateSpeed < minSpeed)
                {
                    moveState = state;
                    minSpeed = stateSpeed;
                }
            }

            UnityEngine.Assertions.Assert.IsNotNull(moveState, string.Format("{0}'s wander script does not have any movement states.", gameObject.name));
            turnSpeed = moveState.turnSpeed;
            moveSpeed = minSpeed;
            ClearAnimatorBools();
            TrySetBool(moveState.animationBool, true);
        }
        void HandleBeginIdle()
        {
            primaryPrey = null;
            var targetWeight = Random.Range(0, totalIdleStateWeight);
            var curWeight = 0;
            foreach (var idleState in idleStates)
            {
                curWeight += idleState.stateWeight;
                if (targetWeight > curWeight)
                    continue;
                idleUpdateTime = Time.time + Random.Range(idleState.minStateTime, idleState.maxStateTime); // 여기서 지정되는 시간만큼 현재 idle state의 지속시간이 정해짐.
                ClearAnimatorBools();
                TrySetBool(idleState.animationBool, true);
                moveSpeed = 0f;
                break;
            }
            idleEvent.Invoke();
        }
        void HandleBeginWander()
        {
            primaryPrey = null;
            var rand = Random.insideUnitSphere * wanderZone;
            var targetPos = startPosition + rand;
            ValidatePosition(ref targetPos);

            wanderTarget = targetPos;
            SetMoveSlow();
        }

        void ValidatePosition(ref Vector3 targetPos)
        {
            if (navMeshAgent)
            {
                NavMeshHit hit;
                if (!NavMesh.SamplePosition(targetPos, out hit, Mathf.Infinity, 1 << NavMesh.GetAreaFromName("Walkable")))
                {
                    Debug.LogError("Unable to sample nav mesh. Please ensure there's a Nav Mesh layer with the name Walkable");
                    enabled = false;
                    return;
                }

                targetPos = hit.position;
            }
        }


        IEnumerator RandomStartingDelay()
        {
            yield return new WaitForSeconds(Random.Range(0f, 2f));
            started = true;
            StartCoroutine(ConstantTicking(Random.Range(.7f, 1f)));
        }

        IEnumerator ConstantTicking(float delay)
        {
            while (true)
            {
                UpdateAI();
                yield return new WaitForSeconds(delay);
            }
            // ReSharper disable once IteratorNeverReturns
        }

        [ContextMenu("This will delete any states you have set, and replace them with the default ones, you can't undo!")]
        public void BasicWanderSetUp()
        {
            MovementState walking = new MovementState(), running = new MovementState();
            IdleState idle = new IdleState();
            AIState attacking = new AIState(), death = new AIState();

            walking.stateName = "Walking";
            walking.animationBool = "isWalking";
            running.stateName = "Running";
            running.animationBool = "isRunning";
            movementStates = new MovementState[2];
            movementStates[0] = walking;
            movementStates[1] = running;


            idle.stateName = "Idle";
            idle.animationBool = "isIdling";
            idleStates = new IdleState[1];
            idleStates[0] = idle;

            attacking.stateName = "Attacking";
            attacking.animationBool = "isAttacking";
            attackingStates = new AIState[1];
            attackingStates[0] = attacking;

            death.stateName = "Dead";
            death.animationBool = "isDead";
            deathStates = new AIState[1];
            deathStates[0] = death;
        }
    }
}