using System.Collections.Generic;

public class SequenceNode : BTNode
{
    private List<BTNode> children = new List<BTNode>();

    public SequenceNode(List<BTNode> nodes)
    {
        children = nodes;
    }

    public override NodeState Tick()
    {
        foreach (var node in children)
        {
            NodeState result = node.Tick();
            if (result != NodeState.Success)
                return result; // Running or Failure
        }
        return NodeState.Success;
    }
}