using UnityEngine;

public class PatrolState : IEnemyState
{
    private EnemyStatePattern enemy;
    private int currentIndex = 0;

    public PatrolState(EnemyStatePattern enemy)
    {
        this.enemy = enemy;
    }

    public void Enter() { }

    public void Exit() { }

    public void Update()
    {
        Transform[] waypoints = enemy.GetPatrolPoints();
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, target.position, enemy.GetMoveSpeed() * Time.deltaTime);

        if (Vector3.Distance(enemy.transform.position, target.position) < 0.1f)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }

        // 플레이어 감지 시 Chase 상태로 전이
        if (Vector3.Distance(enemy.transform.position, enemy.GetPlayer().position) <= enemy.GetDetectRange())
        {
            enemy.SetState(new ChaseState(enemy));
        }
    }
}
