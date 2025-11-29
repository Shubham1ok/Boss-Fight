using UnityEngine;

public class AttackEvents : MonoBehaviour
{
    public SwordHitbox swordHitbox; // assign in inspector

    // These names appear in the animation events dropdown
    public void EnableSwordHitbox()
    {
        if (swordHitbox != null) swordHitbox.EnableHitbox();
    }

    public void DisableSwordHitbox()
    {
        if (swordHitbox != null) swordHitbox.DisableHitbox();
    }
}
