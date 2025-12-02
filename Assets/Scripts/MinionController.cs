using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class MinionController : MonoBehaviour
{
    [Header("Targeting & Combat")]
    public Transform target;
    public float attackRange = 1.6f;
    public int damage = 5;
    public float attackCooldown = 1.2f;
    public float attackAnimLength = 0.6f; // seconds

    [Header("Movement")]
    public float followSmoothTargetLerp = 0.2f;
    public float rotationSpeed = 8f;
    public float stuckCheckInterval = 1.0f;
    public float stuckDistanceThreshold = 0.15f;

    [Header("Misc")]
    public float lifeAfterDeath = 2f;

    // runtime
    private NavMeshAgent agent;
    private Animator animator;
    private float lastAttackTime = -999f;
    private bool isDead = false;
    private bool isAttacking = false;
    private Vector3 lastPos;
    private float lastStuckCheckTime = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = Mathf.Clamp(Random.Range(20, 80), 1, 99);
            agent.angularSpeed = 240f;
        }

        lastPos = transform.position;
        lastStuckCheckTime = Time.time;
    }

    void Update()
    {
        if (isDead) return;

        if (target == null)
        {
            SetMoving(false);
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, target.position);
        if (distToPlayer <= attackRange)
        {
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            if (Time.time - lastAttackTime >= attackCooldown && !isAttacking)
            {
                lastAttackTime = Time.time;
                AttackPlayer();
            }

            FaceTowards(target.position, rotationSpeed * 2f);
            SetMoving(false);
            return;
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;

            Vector3 moveTarget = target.position;
            Vector3 prevDest = agent.hasPath ? agent.destination : transform.position;
            Vector3 smoothTarget = Vector3.Lerp(prevDest, moveTarget, followSmoothTargetLerp);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(smoothTarget, out hit, 1.5f, NavMesh.AllAreas))
            {
                smoothTarget = hit.position;
            }

            agent.SetDestination(smoothTarget);

            FaceTowards(target.position, rotationSpeed);

            if (agent.velocity.magnitude > 0.15f) SetMoving(true);
            else SetMoving(false);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, 3f * Time.deltaTime);
            FaceTowards(target.position, rotationSpeed);
            SetMoving(true);
        }

        if (Time.time - lastStuckCheckTime >= stuckCheckInterval)
        {
            float moved = Vector3.Distance(transform.position, lastPos);
            if (moved < stuckDistanceThreshold)
            {
                TryUnstick();
            }
            lastPos = transform.position;
            lastStuckCheckTime = Time.time;
        }
    }

    private void FaceTowards(Vector3 worldPosition, float turnSpeedLocal = 8f)
    {
        Vector3 lookDir = worldPosition - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude < 0.0001f) return;
        Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeedLocal);
    }

    // helper: does animator have a param with this name + type?
    private bool AnimatorHasParam(string name, AnimatorControllerParameterType type)
    {
        if (animator == null) return false;
        foreach (var p in animator.parameters)
        {
            if (p.name == name && p.type == type) return true;
        }
        return false;
    }

    // helper: does animator have a state on layer 0 with this name?
    private bool AnimatorHasState(string stateName, int layer = 0)
    {
        if (animator == null) return false;
        int hash = Animator.StringToHash(stateName);
        return animator.HasState(layer, hash);
    }

    void SetMoving(bool moving)
    {
        if (animator == null) return;
        if (AnimatorHasParam("IsMoving", AnimatorControllerParameterType.Bool))
            animator.SetBool("IsMoving", moving);
    }

    void SetAttacking(bool attacking)
    {
        if (animator == null) return;
        if (AnimatorHasParam("IsAttacking", AnimatorControllerParameterType.Bool))
            animator.SetBool("IsAttacking", attacking);
    }

    void AttackPlayer()
    {
        if (isAttacking || isDead) return;

        isAttacking = true;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (target != null) FaceTowards(target.position, rotationSpeed * 3f);

        // Play attack animation safely:
        bool played = false;
        if (animator != null)
        {
            // prefer to CrossFade to the named state if it exists
            if (AnimatorHasState("Attack"))
            {
                animator.CrossFadeInFixedTime("Attack", 0.05f, 0, 0f);
                played = true;
            }
            else if (AnimatorHasParam("Attack", AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger("Attack");
                played = true;
            }
            else if (AnimatorHasParam("IsAttacking", AnimatorControllerParameterType.Bool))
            {
                animator.SetBool("IsAttacking", true);
                played = true;
            }
            // else: no matching animation parameter/state found — skip animator call
        }

        // Deal damage regardless of animation
        var ph = target.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeDamage(damage);

        // schedule end of attack
        Invoke(nameof(StopAttackAnim), attackAnimLength);
    }

    void StopAttackAnim()
    {
        isAttacking = false;

        if (animator != null)
        {
            // reset Trigger or Bool if they exist
            if (AnimatorHasParam("Attack", AnimatorControllerParameterType.Trigger)) animator.ResetTrigger("Attack");
            if (AnimatorHasParam("IsAttacking", AnimatorControllerParameterType.Bool)) animator.SetBool("IsAttacking", false);
            SetAttacking(false);
        }

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }
    }

    // Called by MinionHealth when hurt; keeps minion from moving briefly and triggers Hurt safely
    public void PlayHurtStop()
    {
        StartCoroutine(HurtPause());
    }

    private IEnumerator HurtPause()
    {
        if (isDead) yield break;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        if (animator != null && AnimatorHasParam("Hurt", AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger("Hurt");
        }

        yield return new WaitForSeconds(0.25f);

        if (!isDead && agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }
    }

    private void TryUnstick()
    {
        if (agent == null || !agent.enabled) return;

        Vector3 randDir = Random.insideUnitSphere;
        randDir.y = 0f;
        randDir.Normalize();
        Vector3 candidate = transform.position + randDir * 1.0f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidate, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            float ang = i * 45f;
            Vector3 dir = Quaternion.Euler(0, ang, 0) * Vector3.forward;
            Vector3 pos = transform.position + dir * 0.8f;
            if (NavMesh.SamplePosition(pos, out hit, 0.6f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                return;
            }
        }
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if (animator != null)
        {
            if (AnimatorHasParam("Die", AnimatorControllerParameterType.Trigger))
                animator.SetTrigger("Die");
        }

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, lifeAfterDeath);
    }

    void OnDestroy()
    {
        var bb = FindObjectOfType<Blackboard>();
        if (bb != null) bb.currentMinionCount = Mathf.Max(0, bb.currentMinionCount - 1);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
