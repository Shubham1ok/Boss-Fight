using UnityEngine;
using UnityEngine.AI;

public class Action_RangedAttack : ActionNode
{
    private BossController_BT boss;
    private Blackboard bb;
    private float lastTriggerTime = -999f;
    private NavMeshAgent agent;
    private float shotTimeout = 1.5f;
    private float waitStartTime = -999f;
    private bool isWaitingForAnim = false;

    public bool smoothTurn = false;
    public float turnSpeed = 8f;

    public Action_RangedAttack(BossController_BT b, Blackboard blackboard, string n = "Act_Ranged")
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

        if (!bb.playerVisible || dist > boss.rangedRange)
        {
            isWaitingForAnim = false; waitStartTime = -999f;
            if (agent != null) agent.isStopped = false;
            if (boss != null && boss.animator != null) boss.animator.SetBool("IsMoving", true);
            return NodeState.Failure;
        }

        if (isWaitingForAnim)
        {
            if (smoothTurn) FaceSmooth(); else FaceImmediate();

            if (boss.activeProjectile != null)
            {
                isWaitingForAnim = false; waitStartTime = -999f; lastTriggerTime = Time.time;
                return NodeState.Running;
            }

            if (Time.time - waitStartTime > shotTimeout)
            {
                isWaitingForAnim = false; waitStartTime = -999f; lastTriggerTime = Time.time;
                if (agent != null) agent.isStopped = false;
                if (boss.animator != null) boss.animator.SetBool("IsMoving", true);
                return NodeState.Failure;
            }

            if (agent != null) agent.isStopped = true;
            if (boss.animator != null) boss.animator.SetBool("IsMoving", false);
            return NodeState.Running;
        }

        float timeSinceLast = Time.time - lastTriggerTime;
        if (timeSinceLast < boss.rangedFireRate)
        {
            if (smoothTurn) FaceSmooth(); else FaceImmediate();
            return NodeState.Running;
        }

        if (smoothTurn) { if (agent != null) { agent.isStopped = true; agent.ResetPath(); agent.velocity = Vector3.zero; } FaceSmooth(); }
        else { FaceImmediate(); if (agent != null) { agent.isStopped = true; agent.ResetPath(); agent.velocity = Vector3.zero; } }

        if (boss.animator != null)
        {
            boss.animator.SetBool("IsMoving", false);
            boss.animator.SetTrigger("Ranged");
        }

        isWaitingForAnim = true; waitStartTime = Time.time;
        return NodeState.Running;
    }
}
