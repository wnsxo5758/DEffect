using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 4; // 최대 체력
    
    public int CurrentHealth { get; private set; }

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void DecreaseHp(int amount)
    {
        CurrentHealth -= amount;
        
        // 체력이 0 이하로 내려가지 않도록 보정
        if (CurrentHealth < 0)
        {
            CurrentHealth = 0;
        }
        
        Debug.Log("플레이어가 데미지를 입었습니다. 현재 체력: " + CurrentHealth);
        
        // 체력이 0이 되면 죽음 처리
        if (CurrentHealth == 0)
        {
            Death();
        }
    }

    public void RestoreHp(int amount)
    {
        CurrentHealth += amount;
        
        // 체력이 최대 체력을 초과하지 않도록 보정
        if (CurrentHealth > maxHealth)
        {
            CurrentHealth = maxHealth;
        }
        
        Debug.Log("플레이어가 체력을 회복했습니다. 현재 체력: " + CurrentHealth);
    }

    public void Death()
    {
        Debug.Log("플레이어가 사망했습니다.");
        //TODO: 플레이어 사망 구현
    }
}
