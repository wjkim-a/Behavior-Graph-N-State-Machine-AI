using UnityEngine;
public abstract class ConditionNode : BTNode
{
}

public abstract class ActionNode : BTNode
{
}
public class PlayerDetectedCondition : ConditionNode
{
    private Transform enemy;
    private Transform player;
    private float detectionRange;

    public PlayerDetectedCondition(Transform enemy, Transform player, float range)
    {
        this.enemy = enemy;
        this.player = player;
        this.detectionRange = range;
    }

    public override NodeState Tick()
    {
        float distance = Vector3.Distance(enemy.position, player.position);
        return distance <= detectionRange ? NodeState.Success : NodeState.Failure;
    }
}

public class CustomChasePlayerAction : ActionNode
{
    private Transform enemy;
    private Transform player;
    private float speed;

    public CustomChasePlayerAction(Transform enemy, Transform player, float speed)
    {
        this.enemy = enemy;
        this.player = player;
        this.speed = speed;
    }

    public override NodeState Tick()
    {
        Vector3 direction = (player.position - enemy.position).normalized;
        enemy.position += direction * speed * Time.deltaTime;
        return NodeState.Running;
    }
}

public class CustumPatrolAction : ActionNode
{
    private Transform enemy;
    private Transform[] waypoints;
    private float speed;
    private int currentIndex = 0;

    public CustumPatrolAction(Transform enemy, Transform[] waypoints, float speed)
    {
        this.enemy = enemy;
        this.waypoints = waypoints;
        this.speed = speed;
    }

    public override NodeState Tick()
    {
        if (waypoints.Length == 0) return NodeState.Failure;

        Vector3 target = waypoints[currentIndex].position;
        enemy.position = Vector3.MoveTowards(enemy.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(enemy.position, target) < 0.1f)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }

        return NodeState.Running;
    }
}
