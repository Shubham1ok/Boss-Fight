// Projectile.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 6f;
    public float speed = 18f; // fallback speed if RB not used
    public bool useRigidbody = true;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (useRigidbody && rb != null)
        {
            // If already set velocity on instantiation, we keep it. Otherwise apply forward.
            if (rb.linearVelocity.sqrMagnitude < 0.01f)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }
        else
        {
            rb = null;
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    void HandleHit(Collider coll)
    {
        // ignore hitting other projectiles / owner layers if needed
        // Try Boss or PlayerHealth
        var ph = coll.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
        }

        // Optionally check for boss too if friendly fire is needed
        // var boss = coll.GetComponent<BossController_BT>();
        // if (boss != null) {...}

        // small impact effect could go here

        Destroy(gameObject);
    }
}
