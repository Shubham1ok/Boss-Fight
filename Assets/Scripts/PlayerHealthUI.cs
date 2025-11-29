using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Image fillImage;               // assign PlayerHealthFill image here
    public PlayerHealth playerHealth;     // assign PlayerHealth component from player

    void Update()
    {
        if (playerHealth != null)
        {
            float percent = (float)playerHealth.currentHealth / playerHealth.maxHealth;
            fillImage.fillAmount = percent;
        }
    }
}
