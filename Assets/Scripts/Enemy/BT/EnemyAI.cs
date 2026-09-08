using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public Transform[] patrolPoints;
    public float detectRange = 5f;
    public float moveSpeed = 2f;

    private BTNode root;

    private void Start()
    {
        // 트리 구성
        root = new SelectorNode(new List<BTNode>
        {
            new SequenceNode(new List<BTNode>
            {
                new PlayerDetectedCondition(transform, player, detectRange),
                new CustomChasePlayerAction(transform, player, moveSpeed)
            }),
            new CustumPatrolAction(transform, patrolPoints, moveSpeed)
        });
    }

    private void Update()
    {
        root.Tick();
    }
}
