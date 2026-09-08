using System.Collections.Generic;

public class SelectorNode : BTNode
{
    private List<BTNode> children = new List<BTNode>();

    public SelectorNode(List<BTNode> nodes)
    {
        children = nodes;
    }

    public override NodeState Tick()
    {
        foreach (var node in children)
        {
            NodeState result = node.Tick();
            if (result != NodeState.Failure)
                return result; // Success or Running
        }
        return NodeState.Failure;
    }
}