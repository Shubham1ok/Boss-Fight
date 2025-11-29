using System.Collections.Generic;

public class Sequence : BTNode
{
    public List<BTNode> children = new List<BTNode>();
    private int current = 0;

    public Sequence(string n = "Sequence") { name = n; }

    public override void OnStart() { current = 0; }

    public override NodeState Tick()
    {
        while (current < children.Count)
        {
            var state = children[current].Tick();
            if (state == NodeState.Running) return NodeState.Running;
            if (state == NodeState.Failure)
            {
                current = 0;
                return NodeState.Failure;
            }
            current++;
        }
        current = 0;
        return NodeState.Success;
    }
}
