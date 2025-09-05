using UnityEngine;

public class ContainerSpanwer : MonoBehaviour
{
    [Header("컨테이너 오브젝트")]
    [SerializeField]
    private Pool<ContainerObject> containerPool;
    [SerializeField]
    private float spawnCoolTime;
    private void OnEnable()
    {
        InvokeRepeating("Spawn", 0f, spawnCoolTime);
    }

    private void Spawn()
    {
        var contain = containerPool.Get();
    }
}
