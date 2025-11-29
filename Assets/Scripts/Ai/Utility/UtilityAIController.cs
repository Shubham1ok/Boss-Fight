using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility AI controller - polls actions, picks best score, executes.
/// Attach to the Boss (or a dedicated commander object). 
/// Requires UtilityAction components on same GameObject (or you can assign them).
/// </summary>
public class UtilityAIController : MonoBehaviour
{
    [Header("References")]
    public Blackboard blackboard;            // reuse existing blackboard
    public BossController_BT boss;           // optional - allows actions to call boss methods
    [Header("Update")]
    public float thinkInterval = 0.3f;       // how often to re-evaluate (seconds)
    public bool runEveryFrame = false;       // evaluate every frame if true (ignore thinkInterval)

    [Header("Debug")]
    public bool debugLog = false;

    private float lastThink = -999f;
    private UtilityAction[] actions;

    void Awake()
    {
        // find UtilityAction components on the same GameObject
        actions = GetComponents<UtilityAction>();
    }

    void Update()
    {
        if (runEveryFrame)
        {
            Think();
        }
        else
        {
            if (Time.time - lastThink >= thinkInterval)
            {
                Think();
                lastThink = Time.time;
            }
        }
    }

    private void Think()
    {
        if (actions == null || actions.Length == 0) return;

        UtilityAction best = null;
        float bestScore = float.MinValue;

        foreach (var a in actions)
        {
            if (a == null) continue;
            if (!a.IsOffCooldown()) continue; // skip cooling actions

            float s = a.EvaluateScore();
            if (debugLog) Debug.Log($"{name} Eval {a.actionName} -> {s:F2}");
            if (s >= a.minScoreToRun && s > bestScore)
            {
                best = a;
                bestScore = s;
            }
        }

        if (best != null)
        {
            if (debugLog) Debug.Log($"{name} Executing {best.actionName} score {bestScore:F2}");
            best.ExecuteAction();
        }
    }
}
