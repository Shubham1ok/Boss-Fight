using UnityEngine;


public class Action_InvokeUtility : ActionNode
{
    private BossController_BT boss;
    private Blackboard bb;
    private UtilityAIController util;
    private string targetName;

    public Action_InvokeUtility(BossController_BT b, Blackboard blackboard, UtilityAIController u, string actionName, string n = "Act_InvokeUtility")
    {
        boss = b; bb = blackboard; util = u; targetName = actionName; name = n;
    }

    public override NodeState Tick()
    {
        if (boss == null || bb == null)
            return NodeState.Failure;

        if (util == null)
        {
            // try to find on boss
            util = boss.GetComponent<UtilityAIController>();
            if (util == null) return NodeState.Failure;
        }

        // Find the utility action component by actionName and execute it immediately (bypass scoring if needed)
        var comps = util.GetComponents<UtilityAction>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            if (c.actionName == targetName || c.GetType().Name == targetName)
            {
                c.ExecuteAction();
                bb.activeNodeName = name + " (" + targetName + ")";
                return NodeState.Success;
            }
        }

        return NodeState.Failure;
    }
}
