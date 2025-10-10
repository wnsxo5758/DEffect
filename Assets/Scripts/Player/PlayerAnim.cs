using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    // �Ķ���� ���
    private readonly int velocityX = Animator.StringToHash("VelocityX");
    private readonly int velocityY = Animator.StringToHash("VelocityY");
    private readonly int jump = Animator.StringToHash("Jump");
    private readonly int isGrounded = Animator.StringToHash("IsGrounded");
    private readonly int isCrouching = Animator.StringToHash("IsCrouching");
    private readonly int roll = Animator.StringToHash("Roll");
    private readonly int stopRoll = Animator.StringToHash("StopRoll");
    private readonly int isClimbing = Animator.StringToHash("IsClimbing");
    private readonly int isConnected = Animator.StringToHash("IsConnected");
    private readonly int turnValve = Animator.StringToHash("TurnValve");
    private readonly int hasWeapon = Animator.StringToHash("HasWeapon");
    private readonly int attack = Animator.StringToHash("Attack");
    private readonly int throwWeapon = Animator.StringToHash("Throw");
    private readonly int startHeal = Animator.StringToHash("StartHeal");
    private readonly int isHealing = Animator.StringToHash("IsHealing");
    private readonly int manaDrain = Animator.StringToHash("ManaDrain");
    private readonly int hit = Animator.StringToHash("Hit");
    private readonly int skillAcquisition = Animator.StringToHash("SkillAcquisition");
    
    // ���� �̱� �ִϸ��̼�
    private readonly int pullGround = Animator.StringToHash("PullGround");
    private readonly int pullAir = Animator.StringToHash("PullAir");
    private readonly int afterPull = Animator.StringToHash("AfterPull");
    
    // �ڷ���Ʈ �ִϸ��̼�
    private readonly int teleportPre = Animator.StringToHash("TeleportPre");
    private readonly int teleportPost = Animator.StringToHash("TeleportPost");
    
    // ��� ����
    private readonly int revive = Animator.StringToHash("Revive");
    private readonly int death = Animator.StringToHash("Death");
    private readonly int deathMelee = Animator.StringToHash("DeathMelee");
    private readonly int deathRanged = Animator.StringToHash("DeathRanged");
    private readonly int deathPress = Animator.StringToHash("DeathPress");
    private readonly int deathLaser = Animator.StringToHash("DeathLaser");
    private readonly int deathDrown = Animator.StringToHash("DeathDrown");
    private readonly int deathHammer = Animator.StringToHash("DeathHammer");
    private readonly int deathHammerSkill = Animator.StringToHash("DeathHammerSkill");

    private Animator animator; // �ִϸ��̼� 
    private PlayerController controller;
    private MovementRigidbody2D movement; // ������
    private PlayerAttack playerAttack; // �÷��̾� ����
    private PlayerInteraction playerInteraction; // ��ȣ�ۿ�
    private PlayerHp playerHp; // HP

    private float pnpDirection;
    
    // ���� ����
    private bool isPlayingClimbingAnimation = false;
    private bool isPlayingTeleportAnimation = false;

    private DeathData currentDeathData;
    private WeaponPullContext currentPullContext;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponentInParent<PlayerController>();
        movement = GetComponentInParent<MovementRigidbody2D>();
        playerAttack = GetComponentInParent<PlayerAttack>();
        playerInteraction = GetComponentInParent<PlayerInteraction>();
    }

    private void LateUpdate()
    {
        if (movement != null)
        {
            // ���� �ӵ� �� ���� ���� ������Ʈ
            float verticalVelocity = movement.Velocity.y;
            bool currentlyGrounded = movement.IsGrounded;
            
            // ��ٸ� ���°� �ƴ� ���� ������Ʈ
            if (!isPlayingClimbingAnimation)
            {
                animator.SetFloat(velocityY, verticalVelocity);
                
                // ���� ���� ����
                animator.SetBool(isGrounded, currentlyGrounded);
            }
        }
    }
    
    // �̵� �ִϸ��̼� ����
    public void MovementAnim(float x)
    {
        animator.SetFloat(velocityX, Mathf.Abs(x));
    }

    public void JumpAnim()
    {
        animator.SetTrigger(jump);
    }

    public void LadderJumpAnim()
    {
        SetClimbAnim(false);
        
        animator.SetTrigger(jump);
    }
    
    public void SetClimbAnim(bool isOnLadder)
    {
        animator.SetBool(isClimbing, isOnLadder);
        isPlayingClimbingAnimation = isOnLadder;
        
        // ��ٸ��� Ÿ�� ���� ���� �Ķ���� �ʱ�ȭ
        if (isOnLadder)
        {
            animator.SetBool(isGrounded, false);
        }
    }
    
    public void ClimbAnim(float y)
    {
        if (isPlayingClimbingAnimation)
        {
            animator.SetFloat(velocityY, Mathf.Abs(y));
        }
    }
    
    public void SetCrouchAnim(bool crouching)
    {
        animator.SetBool(isCrouching, crouching);
    }
    
    public void CrawlAnim(float x)
    {
        animator.SetFloat(velocityX, Mathf.Abs(x));
    }

    public void StartRollAnim()
    {
        animator.SetTrigger(roll);
        animator.ResetTrigger(stopRoll);
    }

    public void StopRollAnim()
    {
        animator.SetTrigger(stopRoll);
        animator.ResetTrigger(roll);
    }
    
    public void SetHoldAnim(float dir)
    {
        animator.SetBool(isConnected, playerInteraction.IsHolding());
        pnpDirection = dir;
    }
    
    public void PushAndPullAnim(float x)
    {
        animator.SetFloat(velocityX, pnpDirection * x);
    }

    public void SetValveAnim(bool valveState)
    {
        animator.SetBool(turnValve, valveState);
    }

    public void SetManaDrainAnim(bool isDraining)
    {
        if (animator != null)
        {
            animator.SetBool(manaDrain, isDraining);
        }
    }

    public void SetSkillAcquisitionAnim(bool isSkillAcquisition)
    {
        if (animator != null)
        {
            animator.SetBool(skillAcquisition, isSkillAcquisition);
        }
    }
    
    public void SetHasWeapon(bool weapon)
    {
        animator.SetBool(hasWeapon, weapon);
    }

    public void TriggerAttackAnim()
    {
        animator.SetTrigger(attack);
    }

    public void TriggerThrowAnim()
    {
        animator.SetTrigger(throwWeapon);
    }
    
    // ���鿡�� ���� �̱� �ִϸ��̼� ����
    public void StartPullGroundAnim(WeaponPullContext context)
    {
        currentPullContext = context;
        
        if (animator != null)
        {
            animator.SetTrigger(pullGround);
        }
    }
    
    // ���߿��� ���� �̱� �ִϸ��̼� ����
    public void StartPullAirAnim(WeaponPullContext context)
    {
        currentPullContext = context;
        
        if (animator != null)
        {
            animator.SetTrigger(pullAir);
        }
    }

    public void StartAfterPullAnim()
    {
        if (animator != null)
        {
            // ���� �ִϸ��̼��̹Ƿ� Bool �Ķ���� ���
            animator.SetBool(afterPull, true);
        }
    }
    
    public void StopAfterPullAnim()
    {
        if (animator != null)
        {
            // ���� �ִϸ��̼� ����
            animator.SetBool(afterPull, false);
        }
        
        ResetPullAnimationTriggers();
    }

    // �̱� ������ �̺�Ʈ
    private void OnPullDamage()
    {
        if (playerAttack != null)
        {
            playerAttack.OnWeaponPullDamage();
        }
    }
    
    // �ִϸ��̼� �̺�Ʈ���� ȣ��Ǵ� �޼����
    public void OnPullGroundAnimationFinished()
    {
        if (playerAttack != null)
        {
            playerAttack.FinishedPullGroundAnim(currentPullContext);
        }
        
        currentPullContext = null;
        
        ResetPullAnimationTriggers();
        ResetTeleportAnimationTriggers();
    }
    
    public void OnPullAirAnimationFinished()
    {
        if (playerAttack != null)
        {
            playerAttack.FinishedPullAirAnim(currentPullContext);
        }
        
        currentPullContext = null;
        
        ResetPullAnimationTriggers();
        ResetTeleportAnimationTriggers();
    }

    public void StartTeleportAnim()
    {
        if (isPlayingTeleportAnimation) return;
        
        isPlayingTeleportAnimation = true;
        ResetTeleportAnimationTriggers();
        ResetPullAnimationTriggers();
        
        animator.SetTrigger(teleportPre);
    }

    public void FinishTeleportAnim()
    {
        if (isPlayingTeleportAnimation)
        {
            // �ڷ���Ʈ �ִϸ��̼� ���� ����
            isPlayingTeleportAnimation = false;
        
            // �ڷ���Ʈ ���� Ʈ���� �ʱ�ȭ
            ResetTeleportAnimationTriggers();
            ResetPullAnimationTriggers();
        }
    }

    public void EndTeleportAnim()
    {
        if (!isPlayingTeleportAnimation) return;
        
        animator.SetTrigger(teleportPost);
    }

    // �ڷ���Ʈ ���� �ִϸ��̼� �Ϸ�
    private void OnTeleportStartAnim()
    {
        playerAttack?.OnTeleportStartAnim();
    }

    // �ڷ���Ʈ ���� �ִϸ��̼� �Ϸ�
    private void OnTeleportEndAnim()
    {
        isPlayingTeleportAnimation = false;
        playerAttack?.OnTeleportEndAnim();
    }

    public void StartHealAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(startHeal);
        }
    }

    public void OnConnectHealAnimFinished()
    {
        if (controller != null && controller.GetCurrentState() is PlayerStates.VendingMachineHeal healState)
        {
            healState.OnConnectAnimationFinished();
        }
    }
    
    public void SetHealingAnim(bool healing)
    {
        animator.SetBool(isHealing, healing);
    }
    
    public void TriggerHitAnim()
    {
        animator.SetTrigger(hit);
    }
    
    public void TriggerDeathAnim(DeathData deathData)
    {
        currentDeathData = deathData;
        
        TriggerSpecificDeathAnim();
    }

    private void TriggerSpecificDeathAnim()
    {
        ResetAllDeathTrigger();

        switch (currentDeathData.cause)
        {
            case DeathCause.MeleeAttack:
                controller.SpriteFlipX(-currentDeathData.direction);
                animator.SetTrigger(deathMelee);
                break;
            
            case DeathCause.RangedAttack:
                animator.SetTrigger(deathRanged);
                break;
            
            case DeathCause.Press:
                animator.SetTrigger(deathPress);
                break;
            
            case DeathCause.Laser:
                animator.SetTrigger(deathLaser);
                break;
            
            case DeathCause.Drowning:
                animator.SetTrigger(deathDrown);
                break;
            
            case DeathCause.Hammer:
                animator.SetTrigger(deathHammer);
                break;
            
            case DeathCause.HammerSkill:
                animator.SetTrigger(deathHammerSkill);
                break;
            
            case DeathCause.Fall:
            case DeathCause.Environmental:
            default:
                animator.SetTrigger(death);
                break;
        }
    }

    // ���� Ÿ�̹� �̺�Ʈ
    private void HandleAttackEvent()
    {
        playerAttack.HandleAttackCollision();
    }

    private void FinishedMeleeAttackEvent()
    {
        playerAttack.FinishedMeleeAttackAnim();
    }

    private void FinishedThrowWeaponEvent()
    {
        playerAttack.FinishedThrowAnim();
    }

    public void ResetAllAnimationStates()
    {
        animator.SetBool(isGrounded, true);
        animator.SetBool(isCrouching, false);
        animator.SetBool(isClimbing, false);
        animator.SetBool(isConnected, false);
        animator.SetBool(hasWeapon, playerAttack.HasWeapon());
        animator.SetBool(turnValve, false);
        animator.SetBool(afterPull, false);
        animator.SetBool(isHealing, false);
        animator.SetBool(manaDrain, false);
        animator.SetBool(skillAcquisition, false);
        
        animator.ResetTrigger(jump);
        animator.ResetTrigger(roll);
        animator.ResetTrigger(stopRoll);
        animator.ResetTrigger(attack);
        animator.ResetTrigger(throwWeapon);
        animator.ResetTrigger(hit);
        animator.ResetTrigger(startHeal);
        
        ResetPullAnimationTriggers();
        ResetTeleportAnimationTriggers();
        ResetAllDeathTrigger();
        
        isPlayingClimbingAnimation = false;
        isPlayingTeleportAnimation = false;
        
        animator.SetTrigger(revive);
    }

    private void ResetAllDeathTrigger()
    {
        animator.ResetTrigger(death);
        animator.ResetTrigger(deathMelee);
        animator.ResetTrigger(deathRanged);
        animator.ResetTrigger(deathPress);
        animator.ResetTrigger(deathLaser);
        animator.ResetTrigger(deathDrown);
    }

    private void ResetPullAnimationTriggers()
    {
        animator.ResetTrigger(pullGround);
        animator.ResetTrigger(pullAir);
        animator.ResetTrigger(jump);
    }

    private void ResetTeleportAnimationTriggers()
    {
        animator.ResetTrigger(teleportPre);
        animator.ResetTrigger(teleportPost);
        animator.ResetTrigger(jump);
    }
    
    private void RestartGameEvent()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.PlayerDied();
        }
    }
}
