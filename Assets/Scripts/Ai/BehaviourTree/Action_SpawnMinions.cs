using UnityEngine;

public class Action_SpawnMinions : ActionNode
{
    private BossController_BT boss;
    private Blackboard bb;
    private int spawnCount = 1;
    private bool markWave1 = false; // if true, mark wave1 done; else mark wave2 done

    public Action_SpawnMinions(BossController_BT b, Blackboard blackboard, int spawnCount = 1, bool markWave1 = false, string n = "Act_SpawnMinions")
    {
        boss = b;
        bb = blackboard;
        this.spawnCount = Mathf.Max(1, spawnCount);
        this.markWave1 = markWave1;
        name = n;
    }

    public override NodeState Tick()
    {
        if (bb == null || boss == null) return NodeState.Failure;

        // Prevent spawning if we'd exceed maxMinions
        int available = Mathf.Max(0, boss.maxMinions - bb.currentMinionCount);
        if (available <= 0) return NodeState.Success;

        int toSpawn = Mathf.Min(available, spawnCount);

        if (boss.minionPrefab == null)
        {
            Debug.LogWarning("SpawnMinions: minionPrefab not assigned.");
            return NodeState.Failure;
        }

        // STOP THE BOSS IMMEDIATELY, so it roars in place
        var ag = boss.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (ag != null)
        {
            ag.isStopped = true;
            ag.ResetPath();
            ag.velocity = Vector3.zero;
        }

        // Play Roar animation (ensure Animator has a "Roar" trigger)
        if (boss.animator != null)
        {
            boss.animator.SetBool("IsMoving", false);
            boss.animator.SetTrigger("Roar");
        }

        // Spawn minions at the assigned spawn point (or fallback), spread them in a circle with jitter and snap to NavMesh
        Vector3 center;
        if (boss.minionSpawnPoint != null)
            center = boss.minionSpawnPoint.position;
        else
            center = boss.transform.position + boss.transform.forward * 2f + Vector3.up * 0.5f;

        // radius (fallback if boss doesn't have field for some reason)
        float radius = 1.0f;
        var bbt = boss as BossController_BT;
        if (bbt != null) radius = Mathf.Max(0.2f, bbt.minionSpawnRadius);

        for (int i = 0; i < toSpawn; i++)
        {
            // evenly spread angles + small random offset so they don't stack exactly on a circle
            float angle = (360f / Mathf.Max(1, toSpawn)) * i + Random.Range(-12f, 12f);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 jitter = new Vector3(Random.Range(-0.25f, 0.25f), 0f, Random.Range(-0.25f, 0.25f));
            Vector3 spawnPos = center + dir * radius + jitter + Vector3.up * 0.05f;

            // snap to navmesh if available so the spawn point is walkable
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                spawnPos = hit.position;
            }

            GameObject m = GameObject.Instantiate(boss.minionPrefab, spawnPos, Quaternion.identity);

            var minionCtrl = m.GetComponent<MinionController>();
            if (minionCtrl != null && bb.player != null)
            {
                minionCtrl.SetTarget(bb.player);
            }

            bb.currentMinionCount++;
        }


        // mark the appropriate wave done
        if (markWave1) boss.hasRetreatedWave1 = true;
        else boss.hasRetreatedWave2 = true;

        // start boss roar + wait coroutine (boss will remain still during wait)
        float waitSeconds = Random.Range(boss.minWaitAfterRoar, boss.maxWaitAfterRoar);
        boss.DoRoarAndWait(waitSeconds);

        // Make boss invulnerable until minions actually die (no timeout)
        boss.StartInvulnerabilityUntilMinionsGone(0f);

        return NodeState.Success;
    }
}
