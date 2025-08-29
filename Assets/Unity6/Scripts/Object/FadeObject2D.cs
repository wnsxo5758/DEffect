using UnityEngine;
using System;
using System.Collections;


[RequireComponent(typeof(SpriteRenderer))]
public class FadeObject2D : MonoBehaviour
{
    [Header("페이드 설정")]
    [SerializeField, Min(0.01f)] // 최소값은 0.01
    private float fadeDuration = 1.0f;
    private bool isFadedOut; // 페이드가 끝났는가?
    public bool IsFadedOut => isFadedOut;
    public event Action OnFadeComplete;

    private SpriteRenderer renderer2D;
    private Color originalColor; // 기본 색상

    //코루틴 관련

    private Coroutine running;


    private void Awake()
    {
        renderer2D = GetComponent<SpriteRenderer>();
        if (renderer2D == null)
        {
            Debug.LogWarning($"{name}: FadeObject2D에 SpriteRenderer가 없습니다.");
            return;
        }
        originalColor = renderer2D.color; //기본 색상 저장
    }

    private void OnEnable()
    {
        ResetVisual(); // 활성화시 알파값 리셋
    }

    public void BeginFade() // 페이드 시작
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeRoutine());
    }

    private void OnDisable()
    {
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }
    }

    private IEnumerator FadeRoutine() // 페이드 코루틴
    {
        float duration = Mathf.Max(0.01f, fadeDuration); // 런타임 값 에러 방지


        float t = 0f;
        float startAlpha = renderer2D ? renderer2D.color.a : 1f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(t / duration));
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(0f);
        isFadedOut = true;
        running = null;

        OnFadeComplete?.Invoke();
    }


    private void SetAlpha(float a)
    {
        if (!renderer2D) return;
        var c = renderer2D.color;
        c.a = a;
        renderer2D.color = c;
    }

    public void BeginHitFlash(float totalTime, float interval, float lowAlpha = 0.4f)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(HitFlashRoutine(totalTime, interval, lowAlpha));
    }

    private IEnumerator HitFlashRoutine(float totalTime, float interval, float lowAlpha)
    {
        float elapsed = 0f;
        bool low = false;

        while (elapsed < totalTime)
        {
            SetAlpha(low ? lowAlpha : 1f);
            low = !low;
            float step = Mathf.Max(0.01f, interval);
            yield return new WaitForSeconds(step);
            elapsed += step;
        }
        SetAlpha(1f);
        isFadedOut = false;
        running = null;
    }

    private void ResetVisual(bool restoreAlphaToOne = true) // 알파값을 원본 색으로 복원
    {
        if (!renderer2D) return;
        var c = originalColor;
        if (restoreAlphaToOne) c.a = 1f;
        renderer2D.color = c;
        isFadedOut = false;
    }
}

