using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DroneAI : MonoBehaviour
{
    private static readonly int FrontHash = Animator.StringToHash("Front");
    private static readonly int RightHash = Animator.StringToHash("Right");

    // 드론의 상태 상수 정의
    enum DroneState
    {
        Idle,
        Move,
        Attack,
        Damage,
        Die
    }

    // 초기 시작 상태는 Idle로 설정
    DroneState state = DroneState.Idle;
    
    // 대기 상태의 지속 시간
    public float idleDelayTime = 2f;
    // 경과 시간
    private float currentTime;

    public GameObject[] weapons;
    
    // 이동 속도
    public float moveSpeed = 1f;
    // 타워 위치
    private Transform tower;
    private Tower towerComponent;
    // 길 찾기 수해 내비게이션 메시 에이전트
    private NavMeshAgent agent;
    
    // 공격 범위
    public float attackRange = 3f;
    // 공격 상태를 빠져나가는 거리(경계 떨림 방지)
    public float attackExitRange = 3.5f;
    // 공격 지연 시간
    public float attackDelayTime = 2f;
    // 1회 공격 데미지
    public int attackDamage = 2;
    // 드론 사망 시 무기 드랍 확률(0~1)
    [Range(0f, 1f)] public float weaponDropChance = 0.35f;

    [SerializeField] private float hp = 3;
    
    // 폭팔 효과
    private Transform explosion;
    private ParticleSystem expEffect;
    private AudioSource expAudio;
    
    private Animator animator;
    
    public bool isDie = false;


    void Start()
    {
        // 가장 가까운 타워 찾기
        Tower[] allTowers = FindObjectsOfType<Tower>();
        if (allTowers.Length > 0)
        {
            Tower closestTower = allTowers[0];
            float closestDistance = Vector3.Distance(transform.position, closestTower.transform.position);
            
            foreach (var candidateTower in allTowers)
            {
                float distance = Vector3.Distance(transform.position, candidateTower.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTower = candidateTower;
                }
            }
            tower = closestTower.transform;
            towerComponent = closestTower;
        }
        else
        {
            Debug.LogWarning("타워를 찾을 수 없습니다!");
        }

        // NavmeshAgent 컴포넌트 가져오기
        agent = GetComponent<NavMeshAgent>();
        // agent 속도 설정
        agent.speed = moveSpeed;

        explosion = GameObject.Find("Explosion").transform;
        expEffect = explosion.GetComponent<ParticleSystem>();
        expAudio = explosion.GetComponent<AudioSource>();
        
        animator = GetComponentInChildren<Animator>();

        hp += Random.Range(5, 20);
    }

    void Update()
    {
        switch (state)
        {
            case DroneState.Idle:
                Idle();
                break;
            case DroneState.Move:
                Move();
                break;
            case DroneState.Attack:
                Attack();
                break;
            case DroneState.Damage:
                //Damage();
                break;
            case DroneState.Die:
                Die();
                break;
        }
    }

    private void Idle()
    {
        animator.SetFloat(FrontHash, 0f);
        animator.SetFloat(RightHash, 0f);
        // 시간 경과
        currentTime += Time.deltaTime;
        if (currentTime > idleDelayTime)
        {
            // 상태 전환
            state = DroneState.Move;
            currentTime = 0f;
            // agent 활성화
            agent.enabled = true;
        }
    }

    private void Move()
    {
        if(!tower || agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }
        
        // 내비게이샨 할 목적지 설정
        agent.SetDestination(tower.position);

        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        
        // 속도를 정규화해서 -1~1 범위로 유지
        float front = localVelocity.z;
        float right = localVelocity.x;
        
        if (agent.speed > 0.01f)
        {
            front /= agent.speed;
            right /= agent.speed;
        }
        
        // 미세한 떨림 제거
        if (Mathf.Abs(front) < 0.05f) front = 0f;
        if (Mathf.Abs(right) < 0.05f) right = 0f;
        
        animator.SetFloat(FrontHash, front, 0.1f, Time.deltaTime);
        animator.SetFloat(RightHash, right, 0.1f, Time.deltaTime);

        
        
        float distanceToTower = GetHorizontalDistance(transform.position, tower.position);
        if (distanceToTower <= attackRange)
        {
            state = DroneState.Attack;
            currentTime = 0f;
            agent.enabled = false;
        }
    }

    private void Attack()
    {
        if (!tower || towerComponent == null)
        {
            state = DroneState.Idle;
            currentTime = 0f;
            return;
        }

        animator.SetFloat(FrontHash, 0f);
        animator.SetFloat(RightHash, 0f);

        currentTime += Time.deltaTime;
        if (currentTime > attackDelayTime)
        {
            // 공격
            towerComponent.HP -= attackDamage;
            // 경과 시간 초기화
            currentTime = 0;
            
        }
        
        float distanceToTower = GetHorizontalDistance(transform.position, tower.position);
        if (distanceToTower > attackExitRange)
        {
            state = DroneState.Move;
            currentTime = 0f;
            agent.enabled = true;
        }
    }

    private static float GetHorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
    

    private void Die()
    {
        
    }

    public void OnDamageProcess(float damage)
    {
        if (isDie) return;
        
        // 체력을 감소시키고 죽지 않았다면 상태를 데미지로 전환하고 싶다.
        hp -= damage;
        if (hp > 0)
        {
            // 상태를 데미지로 전환
            state = DroneState.Damage;
            StopAllCoroutines();
            // 1. 길찾기 중지
            agent.enabled = false;
        
            // 7. 상태를 Idle로 전환
            state = DroneState.Idle;
            // 8. 경과 시간 초기화
            currentTime = 0;
        }
        else
        {
            isDie = true;
            agent.enabled = false;
            animator.SetTrigger(Animator.StringToHash("Die"));
            StartCoroutine(DamageRoutine());
        }
    }

    IEnumerator DamageRoutine()
    {
        yield return new WaitForSeconds(1);

        // 일정 확률에서는 아무 무기도 드랍하지 않는다.
        if (weapons != null && weapons.Length > 0 && Random.value <= weaponDropChance)
        {
            float[] weights = { 0.1f, 0.1f, 0.1f, 0.2f, 0.5f };
            int usableCount = Mathf.Min(weapons.Length, weights.Length);

            float totalWeight = 0f;
            for (int i = 0; i < usableCount; i++)
            {
                totalWeight += weights[i];
            }

            float pick = Random.value * totalWeight;
            float cumulative = 0f;
            int selectedIndex = 0;
            for (int i = 0; i < usableCount; i++)
            {
                cumulative += weights[i];
                if (pick <= cumulative)
                {
                    selectedIndex = i;
                    break;
                }
            }

            if (weapons[selectedIndex] != null)
            {
                Instantiate(weapons[selectedIndex], transform.position + Vector3.up, Quaternion.identity);
            }
        }

        explosion.position = transform.position;
        expEffect.Play();
        expAudio.Play();
        Destroy(gameObject);
    }
}