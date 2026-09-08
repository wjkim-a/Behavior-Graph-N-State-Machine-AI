using UnityEngine;

public enum EnemyState
{
    Patrol,
    Chase
}

public class EnemyFSM : MonoBehaviour
{
    public Transform player;
    public Transform[] patrolPoints;
    public float detectRange = 5f;
    public float moveSpeed = 2f;

    private EnemyState currentState;
    private int currentPatrolIndex = 0;

    private void Start()
    {
        currentState = EnemyState.Patrol;
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                // 플레이어 감지 시 Chase 상태로 전이
                if (Vector3.Distance(transform.position, player.position) <= detectRange)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                Chase();
                // 플레이어 범위 벗어나면 Patrol 상태로 전이
                if (Vector3.Distance(transform.position, player.position) > detectRange)
                {
                    currentState = EnemyState.Patrol;
                }
                break;
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPatrolIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void Chase()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}
