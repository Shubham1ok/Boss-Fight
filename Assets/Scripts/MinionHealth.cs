using UnityEngine;

[RequireComponent(typeof(MinionController))]
public class MinionHealth : MonoBehaviour
{
    public int maxHealth = 30;
    public int currentHealth;
    //public GameObject hitVFX;      // optional: not used if you don't have VFX
    //public GameObject deathVFX;    // optional: not used if you don't have VFX

    MinionController ctrl;
    Animator anim;
    bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        ctrl = GetComponent<MinionController>();
        anim = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0 || isDead) return;

        // 1) Subtract health first
        currentHealth -= amount;

        // optional: spawn hit VFX (you said you don't use VFX, so safe to keep commented)
        // if (hitVFX != null) Instantiate(hitVFX, transform.position + Vector3.up * 0.8f, Quaternion.identity);

        // 2) If still alive -> play Hurt reaction
        if (currentHealth > 0)
        {
            // play hurt animation (trigger name must match animator)
            if (anim != null)
            {
                anim.SetTrigger("Hurt");
            }

            // tell controller to pause movement/AI briefly if it supports it
            if (ctrl != null)
            {
                ctrl.PlayHurtStop();
            }

            return;
        }

        // 3) Otherwise die - ensure we don't trigger Hurt after this
        Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // optional death VFX
        /*if (deathVFX != null)
            Instantiate(deathVFX, transform.position + Vector3.up * 0.5f, Quaternion.identity);*/

        // clear hurt trigger to avoid it firing after die
        if (anim != null)
        {
            anim.ResetTrigger("Hurt");
        }

        // call minion controller to handle death (plays Die anim, disables nav, etc.)
        if (ctrl != null)
        {
            ctrl.Die();
        }
        else
        {
            Destroy(gameObject, 0.1f);
        }
    }
}
