using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private int damage; // 데미지 
    [SerializeField]
    private float maxLaserLength = 100f;

    [SerializeField]
    private Transform beamTransform;

    [SerializeField]
    private LayerMask obstacleLayer;
    public void SetUp()
    {

    }

    private void Update()
    {
        
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable damageable))
        {
            damageable.DecreaseHp(damage); //피격된 대상에게 데미지 부여
        }
    }

}
