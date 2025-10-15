using UnityEngine;

public class DistanceCheck : MonoBehaviour
{

    [SerializeField]
    private float detectRange;
    [SerializeField]
    private float loseTargetRange;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GizmoDistance()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
    }
}
