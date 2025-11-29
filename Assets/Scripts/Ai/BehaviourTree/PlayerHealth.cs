using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        Debug.Log("Player took damage: " + amount);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            GameManager.Instance.PlayerDied();
           
            Debug.Log("Player Died");

            // Later you can add respawn or death animation
        }
    }
}
