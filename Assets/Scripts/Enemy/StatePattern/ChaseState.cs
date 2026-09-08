using UnityEngine;

public class ChaseState : IEnemyState
{
    private EnemyStatePattern enemy;

    public ChaseState(EnemyStatePattern enemy)
    {
        this.enemy = enemy;
    }

    public void Enter() { }

    public void Exit() { }

    public void Update()
    {
        Vector3 direction = (enemy.GetPlayer().position - enemy.transform.position).normalized;
        enemy.transform.position += direction * enemy.GetMoveSpeed() * Time.deltaTime;

        // 플레이어 범위 벗어나면 Patrol 상태로 전이
        if (Vector3.Distance(enemy.transform.position, enemy.GetPlayer().position) > enemy.GetDetectRange())
        {
            enemy.SetState(new PatrolState(enemy));
        }
    }
}
