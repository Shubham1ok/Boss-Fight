public class Condition_PlayerInRange : BTNode
{
    private Blackboard bb;
    private float range;

    public Condition_PlayerInRange(Blackboard blackboard, float r, string n = "Cond_InRange")
    {
        bb = blackboard; range = r; name = n;
    }

    public override NodeState Tick()
    {
        return (bb.distanceToPlayer <= range) ? NodeState.Success : NodeState.Failure;
    }
}
