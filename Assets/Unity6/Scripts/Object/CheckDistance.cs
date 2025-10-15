using UnityEngine;

public class CheckDistance : MonoBehaviour
{
    public float DistanceToTarget(Transform target)
    {
        if (target == null)
        {
            return float.MaxValue;
        }

        return Vector2.Distance(transform.position, target.position);
    }
}
