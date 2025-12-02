using UnityEngine;

public class Condition_IsLowHealth : BTNode
{
    private BossController_BT boss;
    private Blackboard bb;
    private float lowHealthPercent;

    public Condition_IsLowHealth(BossController_BT b, Blackboard blackboard, float percent, string n = "Cond_LowHealth")
    {
        boss = b;
        bb = blackboard;
        lowHealthPercent = percent;
        name = n;
    }

    public override NodeState Tick()
    {
        if (boss == null || bb == null) return NodeState.Failure;

        

        // Now check HP value
        if (bb.currentHP <= boss.maxHP * lowHealthPercent) return NodeState.Success;

        return NodeState.Failure;
    }
}
