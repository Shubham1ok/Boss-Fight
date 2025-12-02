using UnityEngine;
using UnityEngine.AI;

[AddComponentMenu("Utility/Utility_ChasePlayer")]
public class Utility_ChasePlayer : UtilityAction
{
    [Header("Chase tuning")]
    public float maxDistance = 25f;         // beyond this -> score 0
    public float visibleBonus = 0.6f;       // multiply when visible
    public float executeStopDistance = 2.2f;

    private UtilityAIController controller;
    private Blackboard bb;
    private BossController_BT boss;
    private NavMeshAgent nav;

    void Start()
    {
        controller = GetComponent<UtilityAIController>();
        if (controller != null) { bb = controller.blackboard; boss = controller.boss; }
        boss = boss ?? GetComponent<BossController_BT>();
        nav = GetComponent<NavMeshAgent>();
    }

    public override float EvaluateScore()
    {
        if (bb == null || boss == null || bb.player == null) return 0f;

        float dist = Vector3.Distance(boss.transform.position, bb.player.position);
        if (dist > maxDistance) return 0f;

        // closer player -> higher base score (0..1)
        float baseScore = Mathf.Clamp01(1f - (dist / maxDistance));

        // visibility multiplier
        if (bb.playerVisible) baseScore *= (1f + visibleBonus);
        // small randomness to avoid ties
        baseScore += Random.Range(0f, 0.05f);
        return Mathf.Clamp01(baseScore);
    }

    public override void ExecuteAction()
    {
        if (boss == null || bb == null || bb.player == null) { MarkExecuted(); return; }

        // If too close, prefer not to chase
        float dist = Vector3.Distance(boss.transform.position, bb.player.position);
        if (dist <= executeStopDistance)
        {
            // optionally trigger melee animation if you want here
            if (boss.animator != null) boss.animator.SetBool("IsMoving", false);
            MarkExecuted(); return;
        }

        if (nav != null)
        {
            nav.isStopped = false;
            nav.speed = boss.maxSpeed;
            nav.SetDestination(bb.player.position);
        }
        else
        {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, bb.player.position, boss.maxSpeed * Time.deltaTime);
        }

        if (boss.animator != null) boss.animator.SetBool("IsMoving", true);

        MarkExecuted();
    }
}
