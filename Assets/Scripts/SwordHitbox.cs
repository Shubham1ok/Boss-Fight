using UnityEngine;

// Updated SwordHitbox: damages Boss and Minions, prevents multiple hits per swing
public class SwordHitbox : MonoBehaviour
{
    public int damage = 10;
    public PlayerController player; // assign in inspector
    public bool canHit = false;     // only true during active hit frames

    // Optional: layers to ignore (set in inspector if needed)
    public LayerMask hitMask = ~0;

    // Call these from animation events (or enable/disable from PlayerController)
    public void EnableHitbox()
    {
        canHit = true;
    }
    public void DisableHitbox()
    {
        canHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit) return;                 // only count hits during attack frames
        if (player == null) return;

        // If player is blocking, reduce or cancel damage
        if (player.isBlocking)
        {
            Debug.Log("Blocked!");
            canHit = false; // avoid multiple block notifications in same swing
            return;
        }

        // Optionally filter by layer mask (uncomment if you set layers)
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        // 1) Try minion first
        var minionHealth = other.GetComponent<MinionHealth>();
        if (minionHealth != null)
        {
            minionHealth.TakeDamage(damage);
            //Debug.Log("Hit Minion for " + damage);
            canHit = false; // prevent multiple hits in same swing
            return;
        }

        // 2) If it has a MinionController but no MinionHealth, try calling Die or other method
        var minionCtrl = other.GetComponent<MinionController>();
        if (minionCtrl != null)
        {
            // If you want to apply damage, add MinionHealth. Otherwise call a kill/interrupt.
            // Here we'll call Die() to play death anim (if present)
            minionCtrl.Die();
            Debug.Log("Hit Minion (controller) - killed");
            canHit = false;
            return;
        }

        // 3) Check for Boss by tag or component
        if (other.CompareTag("Boss"))
        {
            BossController_BT boss = other.GetComponent<BossController_BT>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
                Debug.Log("Hit Boss for " + damage);
                canHit = false;
                return;
            }
        }

        // 4) Generic damageable interface fallback (optional)
        var mh = other.GetComponent<MinionHealth>(); // already checked, but safe
        if (mh != null)
        {
            mh.TakeDamage(damage);
            canHit = false;
            return;
        }

        // 5) If no known target, you can still mark that the hit was consumed
        // (prevents hitting multiple colliders in same swing)
        canHit = false;
    }
}
