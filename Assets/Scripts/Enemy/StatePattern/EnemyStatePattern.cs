using UnityEngine;

// 상태 인터페이스
public interface IEnemyState
{
    void Enter();           // 상태 진입 시
    void Exit();            // 상태 종료 시
    void Update();          // 매 프레임 수행
}

// Enemy AI
public class EnemyStatePattern : MonoBehaviour
{
    public Transform player;
    public Transform[] patrolPoints;
    public float detectRange = 5f;
    public float moveSpeed = 2f;

    private IEnemyState currentState;

    // 상태 객체 생성
    private PatrolState patrolState;
    private ChaseState chaseState;

    private void Start()
    {
        patrolState = new PatrolState(this);
        chaseState = new ChaseState(this);

        SetState(patrolState); // 초기 상태
    }

    private void Update()
    {
        currentState.Update();
    }

    public void SetState(IEnemyState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    // 상태 객체에 필요한 데이터 제공
    public Transform[] GetPatrolPoints() => patrolPoints;
    public Transform GetPlayer() => player;
    public float GetDetectRange() => detectRange;
    public float GetMoveSpeed() => moveSpeed;
}
