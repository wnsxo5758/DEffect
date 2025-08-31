using UnityEngine;
using UnityEngine.Pool;

public abstract class PoolItem<T> : MonoBehaviour where T : PoolItem<T>
{
    private IObjectPool<T> _ManagedPool;

    public void SetManagedPool(IObjectPool<T> pool)
    {
        _ManagedPool = pool;
    }

    public void ReleaseToPool()
    {
        _ManagedPool.Release((T)this);
    }
}
