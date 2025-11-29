using UnityEngine;

public class Blackboard : MonoBehaviour
{
    [Header("References")]
    public Transform player;
   /* public Transform[] coverPoints;     // Points used for patrol AND cover*/

    [Header("Sensors / State")]
    public bool playerVisible;
    public float distanceToPlayer;
    public float lastDamageTime;
    public int currentMinionCount;
    public float currentHP;
    public float maxHP;

    [Header("Runtime debug")]
    public string activeNodeName = "";

    // Find the closest cover point to this blackboard's transform
   /* public Transform GetClosestCover()
    {
        if (coverPoints == null || coverPoints.Length == 0) return null;
        Transform closest = null;
        float closestDist = Mathf.Infinity;
        foreach (Transform c in coverPoints)
        {
            if (c == null) continue;
            float dist = Vector3.Distance(transform.position, c.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = c;
            }
        }
        return closest;
    }*/
}
