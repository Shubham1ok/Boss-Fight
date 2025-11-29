using UnityEngine;

/// <summary>
/// Example utility action: spawn N minions when beneficial.
/// Score increases when boss HP is low OR when minion count is low.
/// </summary>
public class Utility_SpawnMinions : UtilityAction
{
    [Header("Spawn Settings")]
    public int spawnCount = 2;
    [Tooltip("Prefer spawning when currentMinionCount <= preferLessThan")]
    public int preferLessThan = 2;
    public float preferBossHpBelow = 0.6f; // more likely to spawn when boss HP < 60%

    // references (optional) - will try to find a controller on the same GameObject
    private UtilityAIController controller;
    private Blackboard bb;
    private BossController_BT boss;

    void Start()
    {
        controller = GetComponent<UtilityAIController>();
        if (controller != null)
        {
            bb = controller.blackboard;
            boss = controller.boss;
        }
    }

    public override float EvaluateScore()
    {
        // defensive
        if (bb == null || boss == null) return 0f;

        // base score
        float score = 0f;

        // more urgent when boss hp is low (e.g., 50% triggers)
        float hpFraction = bb.currentHP / bb.maxHP;
        if (hpFraction <= preferBossHpBelow) score += 0.5f;

        // more urgent if minion count is low
        if (bb.currentMinionCount <= preferLessThan) score += 0.4f;

        // small random to vary decisions
        score += Random.Range(0f, 0.2f);

        return score; // typical 0..1 scale
    }

    public override void ExecuteAction()
    {
        if (controller == null || boss == null || bb == null) return;

        // spawn using boss's existing logic if available, otherwise manual
        // We will try to call boss-spawn behavior if it has a public method; otherwise spawn directly
        // Here we manually spawn to be safe.

        int available = Mathf.Max(0, boss.maxMinions - bb.currentMinionCount);
        int toSpawn = Mathf.Min(spawnCount, available);
        if (toSpawn <= 0)
        {
            MarkExecuted();
            return;
        }

        Vector3 center = boss.minionSpawnPoint != null ? boss.minionSpawnPoint.position : boss.transform.position;
        float radius = Mathf.Max(0.6f, boss.minionSpawnRadius);

        for (int i = 0; i < toSpawn; i++)
        {
            float angle = (360f / Mathf.Max(1, toSpawn)) * i + Random.Range(-10f, 10f);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 spawnPos = center + dir * (radius + Random.Range(0f, 0.4f)) + Vector3.up * 0.05f;

            // snap to navmesh
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
                spawnPos = hit.position;

            GameObject m = Instantiate(boss.minionPrefab, spawnPos, Quaternion.identity);
            var mc = m.GetComponent<MinionController>();
            if (mc != null && bb.player != null) mc.SetTarget(bb.player);
            bb.currentMinionCount++;
        }

        // start invulnerability until minions gone
        boss.StartInvulnerabilityUntilMinionsGone(0f);
        boss.DoRoarAndWait(Random.Range(boss.minWaitAfterRoar, boss.maxWaitAfterRoar));

        MarkExecuted();
    }
}
