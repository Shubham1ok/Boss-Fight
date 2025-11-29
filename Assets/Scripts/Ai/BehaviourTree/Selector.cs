using System.Collections.Generic;

public class Selector : BTNode
{
    public List<BTNode> children = new List<BTNode>();

    public Selector(string n = "Selector") { name = n; }

    public override void OnStart() { }

    public override NodeState Tick()
    {
        foreach (var c in children)
        {
            var s = c.Tick();
            if (s == NodeState.Success) return NodeState.Success;
            if (s == NodeState.Running) return NodeState.Running;
        }
        return NodeState.Failure;
    }
}
