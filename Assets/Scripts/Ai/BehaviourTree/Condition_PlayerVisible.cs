// Condition_PlayerVisible.cs
public class Condition_PlayerVisible : ConditionNode
{
    private Blackboard bb;
    public Condition_PlayerVisible(Blackboard blackboard, string n = "Cond_PlayerVisible")
    {
        bb = blackboard; name = n;
    }

    public override NodeState Tick()
    {
        return bb.playerVisible ? NodeState.Success : NodeState.Failure;
    }
}
