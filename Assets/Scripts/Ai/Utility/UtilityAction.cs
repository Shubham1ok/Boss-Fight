using UnityEngine;


public abstract class UtilityAction : MonoBehaviour
{
    [Header("Utility Action Settings")]
    public string actionName = "UtilityAction";
    [Tooltip("Minimum score required to run this action")]
    public float minScoreToRun = 0.2f;
    [Tooltip("Cooldown after this action executes (seconds)")]
    public float cooldown = 2f;

    [HideInInspector] public float lastExecutionTime = -999f;

    
    public abstract float EvaluateScore();

    
    public abstract void ExecuteAction();

    
    public bool IsOffCooldown()
    {
        return Time.time - lastExecutionTime >= cooldown;
    }

    
    protected void MarkExecuted()
    {
        lastExecutionTime = Time.time;
    }
}
