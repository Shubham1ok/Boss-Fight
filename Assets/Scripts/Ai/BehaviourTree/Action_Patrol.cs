using UnityEngine;
using UnityEngine.AI;

public class Action_Patrol : ActionNode
{
    private BossController_BT boss;
    private Blackboard bb;
    private NavMeshAgent agent;
    private int currentPoint = 0;

    public Action_Patrol(BossController_BT b, Blackboard blackboard, string n = "Act_Patrol")
    {
        boss = b; bb = blackboard; name = n;
        agent = b.GetComponent<NavMeshAgent>();
    }

    public override NodeState Tick()
    {
        // If boss is roaring / waiting after spawn, DO NOT patrol or move
        if (boss.isWaitingForRejoin)
        {
            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
            if (boss.animator != null)
            {
                boss.animator.SetBool("IsMoving", false);
            }

            return NodeState.Failure; // tell selector "patrol cannot run now"
        }

        /*if (bb.coverPoints == null || bb.coverPoints.Length == 0) return NodeState.Failure;*/

        // If there is only one cover point, we want to go there and stay idle when reached
        /*Transform target = bb.coverPoints[currentPoint % bb.coverPoints.Length];*/

        // If only one point: move to it, then stop and idle once reached (stay there)
       /* if (bb.coverPoints.Length == 1)
        {
            // Move towards target if not already close
            if (agent != null)
            {
                // if not yet at destination, set destination
                if (!agent.pathPending && (agent.destination - target.position).sqrMagnitude > 0.01f)
                {
                    agent.isStopped = false;
                    agent.SetDestination(target.position);
                }

                // When close enough, stop and idle
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                    agent.velocity = Vector3.zero;

                    if (boss.animator != null)
                        boss.animator.SetBool("IsMoving", false);

                    bb.activeNodeName = name + " (At Cover Idle)";
                    return NodeState.Running; // stay here (idle)
                }
                else
                {
                    // still moving toward cover
                    if (boss.animator != null)
                        boss.animator.SetBool("IsMoving", true);

                    bb.activeNodeName = name + " (Going to Cover)";
                    return NodeState.Running;
                }
            }
            else
            {
                // No NavMeshAgent fallback (move with transform)
                boss.transform.position = Vector3.MoveTowards(boss.transform.position, target.position, boss.maxSpeed * Time.deltaTime * 0.8f);
                if (Vector3.Distance(boss.transform.position, target.position) < 0.5f)
                {
                    if (boss.animator != null) boss.animator.SetBool("IsMoving", false);
                    bb.activeNodeName = name + " (At Cover Idle)";
                    return NodeState.Running;
                }
                else
                {
                    if (boss.animator != null) boss.animator.SetBool("IsMoving", true);
                    bb.activeNodeName = name + " (Going to Cover)";
                    return NodeState.Running;
                }
            }
        }

        // --- Multiple cover points (original patrol behaviour) ---
        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = boss.maxSpeed * 0.8f;
            agent.SetDestination(target.position);
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
            {
                currentPoint++;
            }
        }
        else
        {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, target.position, boss.maxSpeed * Time.deltaTime * 0.8f);
            if (Vector3.Distance(boss.transform.position, target.position) < 0.5f) currentPoint++;
        }*/

        if (boss.animator != null) boss.animator.SetBool("IsMoving", true);
        bb.activeNodeName = name + " (Patrolling)";
        return NodeState.Running;
    }
}
