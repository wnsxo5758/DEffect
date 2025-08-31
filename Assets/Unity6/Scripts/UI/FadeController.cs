using UnityEngine;
using System.Collections;

public class FadeController : Singleton<FadeController>
{
    [Header("페이드 설정")]
    [SerializeField] public CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1.0f;

    private Coroutine running;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        // 초기 상태: 완전 투명 & 입력 차단 안함
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        // 최상단 정렬 권장 (Canvas 컴포넌트가 같은 오브젝트에 있다고 가정)
        var canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767; // 가장 위
        }
    }

    public IEnumerator FadeOut() => FadeTo(1f);
    public IEnumerator FadeIn() => FadeTo(0f);

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        // 중복 페이드 방지: 한 번에 하나만
        if (running != null) StopCoroutine(running);

        running = StartCoroutine(FadeRoutine(targetAlpha));
        yield return running;
        running = null;
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        fadeCanvasGroup.blocksRaycasts = true;
        float start = fadeCanvasGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // 중요: unscaled
            fadeCanvasGroup.alpha = Mathf.Lerp(start, targetAlpha, t / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
        fadeCanvasGroup.blocksRaycasts = targetAlpha > 0f;
    }
}