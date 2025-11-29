using UnityEngine;

/// <summary>
/// Base class for a utility action. Attach to same GameObject as UtilityAIController.
/// Implement EvaluateScore() and ExecuteAction().
/// UtilityAIController will pick the highest scoring action.
/// </summary>
public abstract class UtilityAction : MonoBehaviour
{
    [Header("Utility Action Settings")]
    public string actionName = "UtilityAction";
    [Tooltip("Minimum score required to run this action")]
    public float minScoreToRun = 0.2f;
    [Tooltip("Cooldown after this action executes (seconds)")]
    public float cooldown = 2f;

    [HideInInspector] public float lastExecutionTime = -999f;

    /// <summary>
    /// Return a score (higher = more desirable).
    /// Should use blackboard/boss/player sensors.
    /// </summary>
    public abstract float EvaluateScore();

    /// <summary>
    /// Called when the controller chooses to execute this action.
    /// Implement action behaviour here (spawn, call boss methods, set flags).
    /// </summary>
    public abstract void ExecuteAction();

    /// <summary>
    /// Helper - whether this action is off cooldown and can run.
    /// </summary>
    public bool IsOffCooldown()
    {
        return Time.time - lastExecutionTime >= cooldown;
    }

    /// <summary>
    /// Call this from ExecuteAction when the action actually runs to start cooldown.
    /// </summary>
    protected void MarkExecuted()
    {
        lastExecutionTime = Time.time;
    }
}
