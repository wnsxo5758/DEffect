using UnityEngine;
using UnityEngine.Events;

public class EnemyHp : MonoBehaviour, IDamageable
{
    [SerializeField]
    private int currentHp; // 현재 체력
    [SerializeField]
    private int maxHp; // 최대 체력
    private bool isDeath;
    private bool isHit;

    FadeObject2D fadeObj;


    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public bool IsDeath => isDeath;
    public bool IsHit => isHit;
    private void Awake()
    {
        currentHp = maxHp;
        fadeObj = GetComponentInChildren<FadeObject2D>();
    }
    public void RestoreHp(int amount) // 체력 증가
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
    }

    private void Hit()
    {
        if(isHit)
        {

        }
    }

    public void DecreaseHp(int amount) // 체력 감소
    {
        currentHp -= amount;

        if (currentHp <= 0)
        {
            Death();
        }
        Hit();
    }

    public void Death() // 사망시, 풀로 반환하도록
    {
        isDeath = true;
        EnemySentence enemySentence = GetComponent<EnemySentence>();
        if (enemySentence != null)
        {
            enemySentence.ShowDeathDialogue();
        }
        fadeObj.BeginFade();
    }
}
