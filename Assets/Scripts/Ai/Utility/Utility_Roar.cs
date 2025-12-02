using UnityEngine;

[AddComponentMenu("Utility/Utility_Roar")]
public class Utility_Roar : UtilityAction
{
    [Header("Roar settings")]
    public float preferBehindHp = 0.55f; // prefer to roar when hp below this
   

    private UtilityAIController controller;
    private Blackboard bb;
    private BossController_BT boss;
    private Utility_SpawnMinions spawnUtility;

    void Start()
    {
        controller = GetComponent<UtilityAIController>();
        if (controller != null)
        {
            bb = controller.blackboard;
            boss = controller.boss;
        }
        boss = boss ?? GetComponent<BossController_BT>();
        spawnUtility = GetComponent<Utility_SpawnMinions>(); // reuse if present
        //cooldown = cooldownAfterRoar;
    }

    public override float EvaluateScore()
    {
        if (bb == null || boss == null) return 0f;

        

        float score = 0f;
        float hpFrac = bb.currentHP / bb.maxHP;

        if (hpFrac <= preferBehindHp) score += 0.5f;
       

        

        // small randomness
        score += Random.Range(0f, 0.1f);

        return Mathf.Clamp01(score);
    }

    public override void ExecuteAction()
    {
        if (boss == null || bb == null) { MarkExecuted(); return; }

        // Trigger Roar animation
        if (boss.animator != null)
        {
                       
            boss.animator.SetBool("IsMoving", false);
            boss.animator.SetTrigger("Roar");
            boss.DoRoarAndWait(Random.Range(boss.minWaitAfterRoar, boss.maxWaitAfterRoar));
        }

        // If we have the Utility_SpawnMinions component, call its ExecuteAction (preferred)
        if (spawnUtility != null)
        {
            spawnUtility.spawnCount=3;
            spawnUtility.ExecuteAction();
            MarkExecuted();
            return;
        }

        

        MarkExecuted();
    }
}
