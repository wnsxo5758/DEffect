using UnityEngine;

/// <summary>
/// 애니메이션 이벤트를 받아서 부모의 PlayerAnimator로 전달하는 중계 컴포넌트
/// Animator 컴포넌트가 자식 오브젝트에 있을 때 사용
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    private PlayerAnimator playerAnimator;

    private void Awake()
    {
        // 부모 오브젝트에서 PlayerAnimator 찾기
        playerAnimator = GetComponentInParent<PlayerAnimator>();

        if (playerAnimator == null)
        {
            Debug.LogError("[AnimationEventReceiver] PlayerAnimator를 부모에서 찾을 수 없습니다!");
        }
    }

    // ========== 애니메이션 이벤트 메서드들 ==========
    // 이 메서드들은 Animation Event에서 호출됩니다

    /// <summary>
    /// 근접 공격 판정 타이밍 (애니메이션 이벤트)
    /// </summary>
    public void OnAttackHit()
    {
        if (playerAnimator != null)
        {
            playerAnimator.OnAttackHit();
        }
    }

    /// <summary>
    /// 근접 공격 애니메이션 종료 (애니메이션 이벤트)
    /// </summary>
    public void OnAttackFinished()
    {
        if (playerAnimator != null)
        {
            playerAnimator.OnAttackFinished();
        }
    }

    /// <summary>
    /// 던지기 실행 타이밍 (애니메이션 이벤트)
    /// </summary>
    public void OnThrowWeapon()
    {
        if (playerAnimator != null)
        {
            playerAnimator.OnThrowWeapon();
        }
    }

    /// <summary>
    /// 던지기 애니메이션 종료 (애니메이션 이벤트)
    /// </summary>
    public void OnThrowFinished()
    {
        if (playerAnimator != null)
        {
            playerAnimator.OnThrowFinished();
        }
    }

    // 향후 추가될 애니메이션 이벤트들도 여기에 중계 메서드로 추가
}
