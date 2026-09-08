using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase Player", story: "[Self] Chase [Player] with [moveSpeed]", category: "Action", id: "e627dadb986603cf136cc3c5798dd079")]
public partial class ChasePlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 direction = (Player.Value.transform.position - Self.Value.transform.position).normalized;
        Self.Value.transform.position += direction * MoveSpeed * Time.deltaTime;
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

