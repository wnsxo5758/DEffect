using UnityEngine;
using UnityEngine.Pool;
using System;

public class Pool<T> : MonoBehaviour where T : PoolItem<T>
{

    [Header("풀 설정")]
    [SerializeField]
    private T prefab; // 프리팹
    [SerializeField]
    private int defaultCapacity = 5; // 기본 생성량 
    [SerializeField]
    private int maxSize = 20; // 최대 값으로 기본 값 : 20

    private ObjectPool<T> objectPool;

    private void Awake()
    {
        objectPool = new ObjectPool<T>(
        OnCreatePoolItem,
        OnGetPoolItem,
        OnReleasePoolItem,
        OnDestroyPoolItem,
        true,
        defaultCapacity,
        maxSize
        );
    }
    private T OnCreatePoolItem() // 풀에서 객체가 새로 생성시
    {
        T item = Instantiate(prefab);
        item.SetManagedPool(objectPool); // 생성된 객체에 풀 참조 전달
        return item;
    }


    private void OnGetPoolItem(T item) // 풀에서 객체를 가져오는 경우
    {
        item.gameObject.SetActive(true);
    }

    private void OnReleasePoolItem(T item) // 풀에서 객체를 돌려보낼때
    {
        item.gameObject.SetActive(false);
    }
    private void OnDestroyPoolItem(T item) // 풀 용량 초과시 객체 파괴
    {
        Destroy(item.gameObject);
    }

    public T Get() // 외부에서 객체를 가져오는 메서드
    {
        return objectPool.Get();
    }
}
