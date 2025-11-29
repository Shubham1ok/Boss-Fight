using UnityEngine;
using UnityEngine.AI;

public class Action_MeleeAttack : ActionNode
{
    private BossController_BT boss;
    private Blackboard bb;
    private float lastAttackTime = -999f;
    private NavMeshAgent agent;
    public bool smoothTurn = false;
    public float turnSpeed = 10f;

    public Action_MeleeAttack(BossController_BT b, Blackboard blackboard, string n = "Act_Melee")
    {
        boss = b; bb = blackboard; name = n;
        agent = b.GetComponent<NavMeshAgent>();
    }

    private void FaceImmediate()
    {
        if (bb.player == null) return;
        Vector3 dir = bb.player.position - boss.transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        boss.transform.rotation = Quaternion.LookRotation(dir.normalized);
    }

    private void FaceSmooth()
    {
        if (bb.player == null) return;
        Vector3 dir = bb.player.position - boss.transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(dir.normalized);
        boss.transform.rotation = Quaternion.Slerp(boss.transform.rotation, target, Mathf.Clamp01(Time.deltaTime * turnSpeed));
    }

    public override NodeState Tick()
    {
        // if boss is in wait mode (roaring) then do not run this action
        if (boss.isWaitingForRejoin) return NodeState.Failure;

        if (bb.player == null) return NodeState.Failure;

        float dist = Vector3.Distance(boss.transform.position, bb.player.position);
        if (!bb.playerVisible || dist > boss.meleeRange + 0.6f)
        {
            if (agent != null) agent.isStopped = false;
            if (boss.animator != null) boss.animator.SetBool("IsMoving", true);
            return NodeState.Failure;
        }

        if (smoothTurn) FaceSmooth(); else FaceImmediate();

        if (agent != null)
        {
            if (!agent.isStopped) { agent.isStopped = true; agent.ResetPath(); agent.velocity = Vector3.zero; }
        }

        if (boss.animator != null) { boss.animator.SetBool("IsMoving", false); boss.animator.applyRootMotion = false; }

        if (Time.time - lastAttackTime >= boss.meleeCooldown)
        {
            lastAttackTime = Time.time;
            if (boss.animator != null)
            {
                try { boss.animator.CrossFadeInFixedTime("Melee", 0.05f, 0, 0f); }
                catch { boss.animator.SetTrigger("Melee"); }
            }

            var ph = bb.player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(boss.meleeDamage);

            bb.activeNodeName = name + " (Attacked)";
        }
        else bb.activeNodeName = name + " (Waiting)";

        return NodeState.Running;
    }
}
