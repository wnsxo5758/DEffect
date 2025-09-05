using UnityEngine;

public class ContainerObject : PoolItem<ContainerObject>
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ObjectDestroy"))
        {
            ReleaseToPool();
        }
    }

}
