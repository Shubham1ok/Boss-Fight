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

        // If this is the 50% wave, check the wave flag
        if (Mathf.Approximately(lowHealthPercent, boss.wave1Threshold))
        {
            if (boss.hasRetreatedWave1) return NodeState.Failure;
        }
        // If this is the 10% wave, check the second flag
        else if (Mathf.Approximately(lowHealthPercent, boss.wave2Threshold))
        {
            if (boss.hasRetreatedWave2) return NodeState.Failure;
        }
        else
        {
            // Generic check: if boss already retreated general flag exists (backward compat), skip
            // (no-op)
        }

        // Now check HP value
        if (bb.currentHP <= boss.maxHP * lowHealthPercent) return NodeState.Success;

        return NodeState.Failure;
    }
}
