using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Detect Player", story: "[Self] Detect [Player] with [detectRange]", category: "Action", id: "91806f1a1472fd7b31b9ec0987c1350f")]
public partial class DetectPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<float> DetectRange;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        float distance = Vector3.Distance(Self.Value.transform.position, Player.Value.transform.position);
        return distance <= DetectRange ? Status.Success : Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

