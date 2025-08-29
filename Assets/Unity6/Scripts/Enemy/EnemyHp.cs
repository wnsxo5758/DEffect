using UnityEngine;
using UnityEngine.Events;

public class EnemyHp : MonoBehaviour, IDamageable
{
    [SerializeField]
    private int currentHp; // 현재 체력
    [SerializeField]
    private int maxHp; // 최대 체력
    public UnityEvent OnDeath;

    private void Awake()
    {
        currentHp = maxHp;
    }
    public void RestoreHp(int amount) // 체력 증가
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
    }

    public void DecreaseHp(int amount) // 체력 감소
    {
        currentHp -= amount;

        if (currentHp <= 0)
        {
            Death();
        }
    }

    public void Death() // 사망시, 풀로 반환하도록
    {
        OnDeath?.Invoke();
    }
}
