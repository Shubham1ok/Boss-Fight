using UnityEngine;
using UnityEngine.AI;

public class Action_ChasePlayer : ActionNode
{
    private BossController_BT boss;
    private Blackboard bb;
    private NavMeshAgent agent;

    public Action_ChasePlayer(BossController_BT b, Blackboard blackboard, string n = "Act_Chase")
    {
        boss = b; bb = blackboard; name = n;
        agent = b.GetComponent<NavMeshAgent>();
    }

    public override NodeState Tick()
    {
        // if boss is in wait mode (roaring) then do not run this action
        if (boss.isWaitingForRejoin) return NodeState.Failure;

        if (bb.player == null) return NodeState.Failure;
        float dist = Vector3.Distance(boss.transform.position, bb.player.position);

        if (!bb.playerVisible)
        {
            if (agent != null) agent.isStopped = false;
            return NodeState.Failure;
        }

        if (dist <= boss.meleeRange + 0.2f)
        {
            if (agent != null) agent.isStopped = true;
            return NodeState.Success;
        }

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = boss.maxSpeed;
            agent.SetDestination(bb.player.position);
        }
        else
        {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, bb.player.position, boss.maxSpeed * Time.deltaTime);
        }

        if (boss.animator != null) boss.animator.SetBool("IsMoving", true);
        bb.activeNodeName = name + " (Chasing)";
        return NodeState.Running;
    }
}
