using UnityEngine;

public abstract class ObjectSpawner<T> : MonoBehaviour where T : PoolItem<T>
{
    [SerializeField]
    private float spawnTime; // 스폰 시간
    [SerializeField]
    private Transform spawnPos; // 스폰 위치


    private void OnEnable()
    {
        InvokeRepeating("Spawn", 0f, spawnTime);
    }
    private void Spawn()
    {
        Pool<T> targetPool = GetTargetPool();

        if (targetPool == null)
        {
            Debug.LogError("Target Pool is not assigned in the inspector!");
            return;
        }

        T newItem = targetPool.Get();

        if (spawnPos == null)
        {
            Debug.LogError("Spawn Position is not assigned!");
            newItem.transform.position = transform.position;
        }
        else
        {
            newItem.transform.position = spawnPos.position;
            newItem.transform.rotation = spawnPos.rotation;
        }
    }
    protected abstract Pool<T> GetTargetPool();
}
