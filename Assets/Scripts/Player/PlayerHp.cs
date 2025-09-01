using System;
using System.Collections;
using UnityEngine;

public class PlayerHp : MonoBehaviour
{
    [Header("ü�� ����")]
    [SerializeField] private int maxHp = 3; // �ִ� ü��
    [SerializeField] private int currentHp; // ���� ü��
    [SerializeField] private float healTime = 0; // ȸ�� �ð�
    [SerializeField] private float invincibilityTime = 0; // �����ð�

    [Header("�ǰ� ����")] 
    [SerializeField] private float hitStunDuration = 0.5f; // �Ϲ� ���� �ð�
    [SerializeField] private float specialStunDuration = 0.25f; // Ư�� ���� ���� �ð�
    
    [Header("UI")]
    [SerializeField] private UIPlayerData uiPlayer;

    [Header("�� ȿ��")]
    [SerializeField]
    private ParticleSystem bloodParticle;

    private PlayerController player;
    private PlayerAnimator playerAnimator;
    private SpriteRenderer spriteRenderer; // �ǰݽ� ���� ������ ���� ��������Ʈ ������
    private Color originColor; //�÷��̾� �ʱ� ����
    private Rigidbody2D rb;
    private MovementRigidbody2D movement;

    private PlayerSound playerSound;

    private float currentHitStunDuration; // ���� �������� ���� �ð�
    private bool isHit; // �ǰ� ���ΰ�
    private bool isDead;
    private bool isHealing; // ȸ�� ���ΰ�
    private bool isInvincible; // �����ΰ�

    private DeathCause lastDeathCause = DeathCause.None;
    private DeathData currentDeathData;

    public System.Action<int, int> OnHpChanged;
    public event Action OnPlayerDeath;
    public event Action<DeathCause> OnPlayerDeathWithCause;
    public bool IsHit { get => isHit; set => isHit = value; }
    public bool IsDead => isDead;
    public DeathCause LastDeathCause => lastDeathCause;
    
    private void Awake()
    {
        currentHp = maxHp;
        playerSound = GetComponentInChildren<PlayerSound>();
        player = GetComponent<PlayerController>();
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        movement = GetComponent<MovementRigidbody2D>();
        rb = GetComponent<Rigidbody2D>();
        originColor = spriteRenderer.color;
    }

    // �Ϲ����� ������ ó�� (������ ����)
    public void DecreaseHp(int damage, DeathData deathData, bool canDodge = false, bool canFreeze = false)
    {
        if (isDead) return;
        
        bool dodged = false;

        if (canDodge && player != null)
        {
            dodged = player.OnAttackReceived(canFreeze);
        }

        // ���� or ȸ�� ���� üũ
        if (isInvincible || dodged) return;
        
        currentHp -= damage;
        ShowBoodEffect();
        if (currentHp <= 0)
        {
            Debug.Log("�÷��̾� ���");
            currentHp = 0;
            currentDeathData = deathData;
            lastDeathCause = deathData.cause;
            Die();
        }
        else
        {
            Debug.Log("�÷��̾�� " + damage + "������");
            playerSound.HitSound();
            HandleHit(Vector2.zero);
        }

        uiPlayer.SetHpAll(currentHp); // ���⼭ ��ü ����
    }

    // �˹��� ���Ե� ������ ó��
    public void DecreaseHp(int damage, Vector2 knockBack, DeathData deathData, bool canDodge = false, bool freeze = false)
    {
        if (isDead) return;
        
        bool dodged = false;

        if (canDodge && player != null)
        {
            dodged = player.OnAttackReceived(freeze);
        }
        
        if (isInvincible || dodged) return;
        
        currentHp -= damage;
        ShowBoodEffect();
        if (currentHp <= 0)
        {
            Debug.Log("�÷��̾� ���");
            
            currentHp = 0;
            currentDeathData = deathData;
            lastDeathCause = deathData.cause;
            Die();
        }
        else
        {
            playerSound.HitSound();
            Debug.Log("�÷��̾�� " + damage + "������");
            
            HandleHit(knockBack);
        }

        uiPlayer.SetHpAll(currentHp);
    }
    
    private void HandleHit(Vector2 knockBack)
    {
        var currentState = player.GetCurrentState();
        bool isSpecialState = currentState is not PlayerStates.Idle and not PlayerStates.Run 
            and not PlayerStates.Jump and not PlayerStates.Crawl;
        
        // ���� �ð� ����
        currentHitStunDuration = isSpecialState ? specialStunDuration : hitStunDuration;
        isHit = true;
        
        //�˹� ó��
        if (!isSpecialState && knockBack != Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockBack, ForceMode2D.Impulse);
        }
        
        // �ǰ� ȿ��
        OnInvincibility(2f);
        
        // ���� ��ȯ �Ǵ� ���� ó��
        if (isSpecialState)
        {
            StartCoroutine(SpecialStateStun());
        }
        else
        {
            if (knockBack == Vector2.zero)
            {
                player.UpdateMove(0);
            }
            player.ChangeState(new PlayerStates.Hit());
        }
    }
    
    public void Die()
    {
        isDead = true;
        currentHp = 0;
        playerSound.DeadthSound();
        ApplyDeathPhysics();
        playerAnimator.TriggerDeathAnim(currentDeathData);
        OnPlayerDeath?.Invoke();
        OnPlayerDeathWithCause?.Invoke(lastDeathCause);
        uiPlayer.SetDeathUI();
    }
    
    public void OnInvincibility(float time) // ��������
    {
        if (isInvincible)
        {
            invincibilityTime += time;
        }
        else
        {
            invincibilityTime = time;
            StartCoroutine(nameof(Invincibility));
        }
    }

    private IEnumerator Invincibility() // ��������, ĳ���� �����̴� ȿ��
    {
        isInvincible = true;
        float blinkSpeed = 10;

        while (invincibilityTime > 0)
        {
            invincibilityTime -= Time.deltaTime;
            Color color = spriteRenderer.color;
            color.a = Mathf.SmoothStep(0, 1, Mathf.PingPong(Time.time * blinkSpeed, 1));
            spriteRenderer.color = color;

            yield return null;
        }

        spriteRenderer.color = originColor;
        isInvincible = false;
    }

    private IEnumerator SpecialStateStun()
    {
        float timer = currentHitStunDuration;

        movement.MoveTo(0);

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        
        isHit = false;
    }

    private void ApplyDeathPhysics()
    {
        switch (currentDeathData.cause)
        {
            case DeathCause.Press:
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.isKinematic = true;
                break;
            
            case DeathCause.Drowning:
            case DeathCause.Fall:
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true;
                break;
            
            default:
                rb.linearVelocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                break;
        }
    }
    
    public void ResetDeathState()
    {
        isDead = false;
        isHit = false;
        lastDeathCause = DeathCause.None;
        currentDeathData = new DeathData();
        
        // ü���� �ִ� ü������ ����
        SetHp(GetMaxHp());
        
        // ���� ���� ����
        uiPlayer.ResetDeathUI();
        uiPlayer.SetHpAll(currentHp);
    }

    public void TakeFallDamage(int damage, bool water)
    {
        DeathData deathData = water ? new DeathData(DeathCause.Drowning) : new DeathData(DeathCause.Fall);
        DecreaseHp(damage, deathData);
        
        if (uiPlayer != null)
        {
            uiPlayer.SetHpAll(currentHp);
        }
    }
    
    public float GetCurrentHitStunDuration()
    {
        return currentHitStunDuration;
    }

    public int GetMaxHp()
    {
        return maxHp;
    }
    
    public int GetCurrentHp()
    {
        return currentHp;
    }

    public void SetHp(int newHp)
    {
        currentHp = newHp;
    }
    
    public int IncreaseHp(int amount)  // ü�� ȸ��
    {
        if (amount <= 0) return 0;

        int previousHp = currentHp;
        currentHp = Mathf.Min(currentHp + amount, maxHp);
        
        int actualHealed = currentHp - previousHp;

        OnHpChanged?.Invoke(currentHp, maxHp);
        
        Debug.Log($"ü�� ȸ��: {actualHealed} (����: {currentHp}/{maxHp})");
        uiPlayer.SetHpAll(currentHp);
        return actualHealed;
    }

    public void TriggerDebugDeath()
    {
        if (isDead) return;

        DeathData deathData = new DeathData(DeathCause.Environmental);
        currentDeathData = deathData;
        lastDeathCause = deathData.cause;
        Die();
    }

    private void ShowBoodEffect()
    {
        Debug.Log("��ƼŬ ����");
        bloodParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        bloodParticle.Play();
        bloodParticle.Emit(1);
    }
}
