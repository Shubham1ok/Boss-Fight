using UnityEngine;

/// <summary>
/// Utility action which decides to force the boss to prefer melee or ranged
/// This example calls BossController_BT public methods or sets flags (you may need to adapt).
/// </summary>
public class Utility_AttackChoice : UtilityAction
{
    [Header("Attack Tuning")]
    public float preferMeleeBelowDistance = 3.0f; // if player < this -> melee score increases
    public float preferRangedAboveDistance = 6.0f; // if player > this -> ranged score increases

    private UtilityAIController controller;
    private Blackboard bb;
    private BossController_BT boss;

    void Start()
    {
        controller = GetComponent<UtilityAIController>();
        if (controller != null) { bb = controller.blackboard; boss = controller.boss; }
    }

    public override float EvaluateScore()
    {
        if (bb == null || boss == null || bb.player == null) return 0f;

        float dist = Vector3.Distance(boss.transform.position, bb.player.position);
        float score = 0f;

        // if close, encourage melee
        if (dist <= preferMeleeBelowDistance) score += 0.7f;

        // if medium range, encourage ranged
        if (dist >= preferRangedAboveDistance) score += 0.6f;

        // small randomness
        score += Random.Range(0f, 0.15f);
        return score;
    }

    public override void ExecuteAction()
    {
        if (boss == null) { MarkExecuted(); return; }

        // Example effects:
        // If player is very close -> trigger an immediate melee attack (You already have Action_MeleeAttack, so call some method or set a flag)
        float dist = bb != null && bb.player != null ? Vector3.Distance(boss.transform.position, bb.player.position) : 999f;

        if (dist <= preferMeleeBelowDistance)
        {
            // Try to make boss immediately attempt a melee attack:
            // We call boss.DoRoarAndWait with zero or trigger a melee method if present.
            // If your boss has a public MeleeNow() you can call it. Otherwise we can do a small forced set:
            var meleeAction = boss.GetComponent<Action_MeleeAttack>();
            if (meleeAction != null)
            {
                // call the node's logic by toggling blackboard or directly calling a method
                // But we probably don't have direct public API, so we can rotate boss and trigger the melee animation:
                boss.animator.SetTrigger("Melee");
            }
        }
        else
        {
            // force ranged shot now
            boss.SpawnProjectileFromAnimation();
        }

        MarkExecuted();
    }
}
