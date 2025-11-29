using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class BossSensesDebug : MonoBehaviour
{
    public BossController_BT boss;      // assign boss (or leave null to auto-find on same object)
    public Blackboard blackboard;       // optional: assign to read sensors/state
    public Color visionColor = new Color(0.2f, 0.8f, 1f, 0.25f);
    public Color visionArcColor = new Color(0.2f, 0.8f, 1f, 1f);
    public Color hearingColor = new Color(1f, 0.85f, 0.2f, 0.12f);
    public Color meleeColor = new Color(1f, 0.3f, 0.3f, 0.25f);
    public Color rangedColor = new Color(0.6f, 0.2f, 1f, 0.15f);
    public Color coverColor = new Color(0.4f, 1f, 0.4f, 0.3f);
    public bool showLabels = true;
    public bool showSightRays = true;
    public int arcSegments = 32;

    void Reset()
    {
        // try auto-assign
        if (boss == null) boss = GetComponent<BossController_BT>();
        if (blackboard == null && boss != null) blackboard = boss.blackboard;
    }

    void OnValidate()
    {
        if (boss == null) boss = GetComponent<BossController_BT>();
        if (blackboard == null && boss != null) blackboard = boss.blackboard;
    }

    void OnDrawGizmos()
    {
        if (boss == null && blackboard == null) return;

        Transform t = boss != null ? boss.transform : transform;
        Blackboard bb = blackboard != null ? blackboard : (boss != null ? boss.blackboard : null);

        // Draw hearing radius as transparent sphere
        if (boss != null)
        {
            Gizmos.color = hearingColor;
            Gizmos.DrawSphere(t.position, boss.hearingRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(t.position, boss.hearingRadius);
        }

        // Draw melee range
        if (boss != null)
        {
            Gizmos.color = meleeColor;
            Gizmos.DrawWireSphere(t.position, boss.meleeRange);
        }

        // Draw ranged range
        if (boss != null)
        {
            Gizmos.color = rangedColor;
            Gizmos.DrawWireSphere(t.position, boss.rangedRange);
        }

        // Draw vision cone (filled by many lines)
        if (boss != null)
        {
            Vector3 forward = t.forward;
            float halfAngle = boss.visionAngle * 0.5f;
            float range = boss.visionRange;

            // draw outline arc
            Gizmos.color = visionArcColor;
            Vector3 prev = Quaternion.Euler(0, -halfAngle, 0) * forward * range;
            prev += t.position;
            for (int i = 1; i <= arcSegments; i++)
            {
                float ang = -halfAngle + (i * (boss.visionAngle / arcSegments));
                Vector3 next = Quaternion.Euler(0, ang, 0) * forward * range;
                next += t.position;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }

            // draw cone lines
            Gizmos.color = visionArcColor;
            Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward * range;
            Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward * range;
            Gizmos.DrawLine(t.position, t.position + leftDir);
            Gizmos.DrawLine(t.position, t.position + rightDir);

            // optional sight rays to player
            if (bb != null && bb.player != null && showSightRays)
            {
                Gizmos.color = bb.playerVisible ? new Color(0f, 1f, 0f, 0.9f) : new Color(1f, 0f, 0f, 0.9f);
                Gizmos.DrawLine(t.position + Vector3.up * 1.2f, bb.player.position + Vector3.up * 1.2f);
                // draw small sphere at the player
                Gizmos.DrawSphere(bb.player.position + Vector3.up * 1.2f, 0.08f);
            }
        }

        // draw cover points if blackboard has them
        /*if (bb != null && bb.coverPoints != null)
        {
            Gizmos.color = coverColor;
            foreach (var c in bb.coverPoints)
            {
                if (c == null) continue;
                Gizmos.DrawWireSphere(c.position, 0.3f);
                Gizmos.DrawLine(t.position, c.position);
            }
        }*/

#if UNITY_EDITOR
        // Draw labels in Scene view for clarity
        if (showLabels && UnityEditor.SceneView.lastActiveSceneView != null)
        {
            string label = $"Boss: {(boss != null ? boss.name : gameObject.name)}\n";
            if (bb != null)
            {
                label += $"HP: {bb.currentHP}/{(bb.maxHP > 0 ? bb.maxHP : 0)}\n";
                label += $"PlayerVisible: {bb.playerVisible}\n";
                label += $"Dist: {Mathf.Round((bb.distanceToPlayer) * 10f) / 10f}\n";
                label += $"ActiveNode: {bb.activeNodeName}\n";
                label += $"Minions: {bb.currentMinionCount}\n";
            }
            Handles.Label(t.position + Vector3.up * 2.5f, label);
        }
#endif
    }
}
