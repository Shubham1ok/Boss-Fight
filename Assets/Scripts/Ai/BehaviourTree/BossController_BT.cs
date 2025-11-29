using System;
using UnityEngine;
using UnityEngine.AI;

public class BossController_BT : MonoBehaviour
{
    [Header("References")]
    public Blackboard blackboard;
    public Transform firePoint;
    public GameObject minionPrefab;
    public GameObject rangedProjectile;
    [Header("Minion Spawn")]
    public Transform minionSpawnPoint;   // assign empty transform here

    [Header("Stats (exposed)")]
    public float maxSpeed = 3.5f;
    public float visionRange = 20f;
    public float visionAngle = 90f;
    public float hearingRadius = 8f;
    public float meleeRange = 2f;
    public float rangedRange = 15f;
    // retreatThresholdHP left for backward compat, but we use explicit thresholds below
    public float retreatThresholdHP = 0.35f;

    // NEW: thresholds & per-wave spawn counts
    [Header("Retreat Waves")]
    [Range(0f, 1f)] public float wave1Threshold = 0.5f; // 50%
    [Range(0f, 1f)] public float wave2Threshold = 0.10f; // 10%
    public int wave1SpawnCount = 2;
    public int wave2SpawnCount = 3;

    [Header("Minion Spawn Settings")]
    public float minionSpawnRadius = 1.2f;

    public int maxMinions = 6;
    public float meleeCooldown = 2f;
    public int meleeDamage = 10;
    public float rangedFireRate = 1.0f;
    public float rangedProjectileSpeed = 18f;

    [Header("Retreat/Wait")]
    [HideInInspector] public bool isWaitingForRejoin = false; // boss is waiting after spawning minions
    public float minWaitAfterRoar = 3f; // base
    public float maxWaitAfterRoar = 5f; // base

    [Header("Death & Feedback")]
    public float maxHP = 100f;
    [SerializeField] private float currentHP = 100f;
    public GameObject deathVFXPrefab;
    public GameObject lootPrefab;
    public Transform lootSpawnPoint;
    public bool destroyOnDeath = false;
    public float destroyDelay = 3f;

    [Header("Shield Visual")]
    public GameObject shieldObject;
    [Header("Invulnerability")]
    [HideInInspector] public bool isInvulnerable = false;    // when true, boss ignores damage

    public float invulnerableTimeoutFallback = 12f;         // not used by default when waiting for minions

    [Header("Runtime")]
    public Animator animator;

    // per-wave done flags (so wave triggers only once each)
    [Header("Internal Wave Flags")]
    public bool hasRetreatedWave1 = false; // 50% wave done
    public bool hasRetreatedWave2 = false; // 10% wave done

    // public tracking of last spawned projectile (set by SpawnProjectileFromAnimation)
    [HideInInspector] public GameObject activeProjectile = null;

    // BT root
    private BTNode root;
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (agent != null) agent.speed = maxSpeed;
        if (blackboard == null) { Debug.LogError("Assign Blackboard on BossController_BT."); enabled = false; return; }

        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
        blackboard.currentHP = currentHP;
        blackboard.maxHP = maxHP;

        BuildBehaviourTree();
    }

    void Update()
    {
        UpdateSensors();

        if (currentHP <= 0f) return;

        if (root != null)
        {
            root.Tick();
        }
    }

    private void UpdateSensors()
    {
        if (blackboard.player != null)
        {
            blackboard.distanceToPlayer = Vector3.Distance(transform.position, blackboard.player.position);
            blackboard.playerVisible = CheckSight(blackboard.player);
        }
        else
        {
            blackboard.playerVisible = false;
            blackboard.distanceToPlayer = float.MaxValue;
        }
        blackboard.currentHP = currentHP;
    }

    private bool CheckSight(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        float d = dir.magnitude;
        if (d > visionRange) return false;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > visionAngle * 0.5f) return false;

        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 1.2f;
        if (Physics.Raycast(origin, dir.normalized, out hit, visionRange))
        {
            if (hit.transform == target) return true;
            return false;
        }
        return false;
    }

    private void BuildBehaviourTree()
    {
        Selector rootSel = new Selector("RootSelector");

        // --- Wave 1 (50%) Sequence: low health -> roar+spawn+wait (no move-to-cover)
        Sequence wave1Seq = new Sequence("Wave1_RetreatAndSummon");
        var lowHealth50 = new Condition_IsLowHealth(this, blackboard, wave1Threshold, "Cond_LowHp_50");
        wave1Seq.children.Add(lowHealth50);
        wave1Seq.children.Add(new Action_SpawnMinions(this, blackboard, wave1SpawnCount, true, "Act_SpawnMinions50")); // spawnCount, markWave1 = true

        // --- Wave 2 (10%) Sequence
        Sequence wave2Seq = new Sequence("Wave2_RetreatAndSummon");
        var lowHealth10 = new Condition_IsLowHealth(this, blackboard, wave2Threshold, "Cond_LowHp_10");
        wave2Seq.children.Add(lowHealth10);
        wave2Seq.children.Add(new Action_SpawnMinions(this, blackboard, wave2SpawnCount, false, "Act_SpawnMinions10")); // spawnCount, markWave1 = false -> marks wave2

        // Melee Sequence
        Sequence meleeSeq = new Sequence("MeleeSequence");
        meleeSeq.children.Add(new Condition_PlayerInRange(blackboard, meleeRange, "Cond_InMeleeRange"));
        meleeSeq.children.Add(new Condition_PlayerVisible(blackboard, "Cond_PlayerVisForMelee"));
        var meleeAction = new Action_MeleeAttack(this, blackboard, "Act_Melee");
        meleeAction.smoothTurn = true;
        meleeAction.turnSpeed = 8f;
        meleeSeq.children.Add(meleeAction);

        // Ranged Sequence
        Sequence rangedSeq = new Sequence("RangedSequence");
        rangedSeq.children.Add(new Condition_PlayerInRange(blackboard, rangedRange, "Cond_InRangedRange"));
        rangedSeq.children.Add(new Condition_PlayerVisible(blackboard, "Cond_PlayerVisForRanged"));
        var rangedAction = new Action_RangedAttack(this, blackboard, "Act_Ranged");
        rangedAction.smoothTurn = true;
        rangedAction.turnSpeed = 6f;
        rangedSeq.children.Add(rangedAction);

        // Chase + Patrol
        var chase = new Action_ChasePlayer(this, blackboard, "Act_Chase");
        var patrol = new Action_Patrol(this, blackboard, "Act_Patrol");

        // Order: Wave1 -> Wave2 -> Melee -> Ranged -> Chase -> Patrol
        rootSel.children.Add(wave1Seq);
        rootSel.children.Add(wave2Seq);
        rootSel.children.Add(meleeSeq);
        rootSel.children.Add(rangedSeq);
        rootSel.children.Add(chase);
        rootSel.children.Add(patrol);

        root = rootSel;
    }

    // damage system
    public void TakeDamage(int amount)
    {
        // If boss currently invulnerable due to minions, ignore damage
        if (isInvulnerable) return;

        if (currentHP <= 0f) return;
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
        blackboard.currentHP = currentHP;
        blackboard.lastDamageTime = Time.time;
        if (animator != null && currentHP > 0f) animator.SetTrigger("Hurt");
        if (currentHP <= 0f) Die();
    }


    private void Die()
    {
        if (animator != null) animator.SetTrigger("Die");
        if (deathVFXPrefab != null) Instantiate(deathVFXPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        if (lootPrefab != null)
        {
            Transform spawn = lootSpawnPoint != null ? lootSpawnPoint : transform;
            Instantiate(lootPrefab, spawn.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        var cols = GetComponentsInChildren<Collider>();
        foreach (var c in cols) c.enabled = false;
        if (agent != null) { agent.isStopped = true; agent.enabled = false; }
        enabled = false;
        if (destroyOnDeath) Destroy(gameObject, destroyDelay);
        GameManager.Instance.BossDied();
    }

    // Public helper � start boss invulnerability while minions exist or until timeout
    public void StartInvulnerabilityUntilMinionsGone(float timeout)
    {
        StopCoroutineInvulnerability();
        _invulCoroutine = StartCoroutine(InvulnerabilityCoroutine(timeout));
    }

    private Coroutine _invulCoroutine = null;
    private void StopCoroutineInvulnerability()
    {
        if (_invulCoroutine != null) { StopCoroutine(_invulCoroutine); _invulCoroutine = null; }
    }

    private System.Collections.IEnumerator InvulnerabilityCoroutine(float timeout)
    {
        isInvulnerable = true;

        if (shieldObject != null) shieldObject.SetActive(true);

        float start = Time.time;
        bool useTimeout = timeout > 0f;

        while (true)
        {
            if (blackboard != null && blackboard.currentMinionCount <= 0)
                break;

            if (useTimeout && (Time.time - start >= timeout))
                break;

            yield return null;
        }

        isInvulnerable = false;

        if (shieldObject != null) shieldObject.SetActive(false);

        _invulCoroutine = null;
    }

    // This is called by the ranged animation event to spawn the projectile precisely on-frame
    public void SpawnProjectileFromAnimation()
    {
        if (rangedProjectile == null || firePoint == null) { Debug.LogWarning("SpawnProjectileFromAnimation missing prefab or firePoint."); return; }
        Vector3 aimPos = blackboard != null && blackboard.player != null ? (blackboard.player.position + Vector3.up * 1f) : (firePoint.position + transform.forward);
        Vector3 dir = (aimPos - firePoint.position).normalized;
        GameObject p = Instantiate(rangedProjectile, firePoint.position, Quaternion.LookRotation(dir));
        var rb = p.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = dir * rangedProjectileSpeed;
        else
        {
            var proj = p.GetComponent<Projectile>();
            if (proj != null) proj.speed = rangedProjectileSpeed;
        }
        activeProjectile = p;
    }

    // Called by spawn action to make boss play roar and wait, then rejoin fight
    public void DoRoarAndWait(float duration)
    {
        if (isWaitingForRejoin) return;
        StartCoroutine(DoRoarAndWaitCoroutine(duration));
    }

    private System.Collections.IEnumerator DoRoarAndWaitCoroutine(float duration)
    {
        isWaitingForRejoin = true;

        var ag = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (ag != null) { ag.isStopped = true; ag.ResetPath(); ag.velocity = Vector3.zero; }

        yield return new WaitForSeconds(duration);

        if (ag != null) ag.isStopped = false;

        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
            try { animator.CrossFadeInFixedTime("Run", 0.08f, 0, 0f); }
            catch { animator.ResetTrigger("Roar"); }
            animator.ResetTrigger("Roar");
        }

        isWaitingForRejoin = false;
    }

    public float GetCurrentHP() { return currentHP; }

    void OnGUI()
    {
        if (blackboard == null) return;
        GUI.Box(new Rect(10, 10, 360, 120),
            "Active Node: " + blackboard.activeNodeName + "\n" +
            "HP: " + currentHP + "/" + maxHP + "\n" +
            "PlayerVisible: " + blackboard.playerVisible + "\n" +
            "Dist to Player: " + Mathf.Round(blackboard.distanceToPlayer * 10f) / 10f + "\n" +
            "Minions: " + blackboard.currentMinionCount + "\n" +
            "Wave1Done: " + hasRetreatedWave1 + " Wave2Done: " + hasRetreatedWave2);
    }
}
