using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;      // the fill image reference
    public BossController_BT boss;
    public MinionHealth health;
    void Update()
    {
        if (boss != null && fillImage != null)
        {
            float percent = boss.GetCurrentHP() / boss.maxHP;
            fillImage.fillAmount = percent;
        }
        if (health != null && fillImage != null)
        {
            float percent = (float)health.currentHealth / health.maxHealth;
            fillImage.fillAmount = percent;
        }
    }
}
