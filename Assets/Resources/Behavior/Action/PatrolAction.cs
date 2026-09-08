using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol", story: "[Self] Patrol [PatrolPoints] Patrol with [moveSpeed]", category: "Action", id: "ad3c383046ddf3ed6eed4ed70920d0d4")]
public partial class PatrolAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<List<GameObject>> PatrolPoints;
    [SerializeReference] public BlackboardVariable<float> moveSpeed;
    private int currentIndex = 0;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (PatrolPoints.Value.Count == 0) return Status.Failure;

        Vector3 target = PatrolPoints.Value[currentIndex].transform.position;
        Self.Value.transform.position = Vector3.MoveTowards(Self.Value.transform.position, target, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(Self.Value.transform.position, target) < 0.1f)
        {
            currentIndex = (currentIndex + 1) % PatrolPoints.Value.Count;
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

