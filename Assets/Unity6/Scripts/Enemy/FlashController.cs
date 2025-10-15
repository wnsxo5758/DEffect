using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;
using Unity.VisualScripting;

[RequireComponent(typeof(SpriteRenderer))]
public class FlashController : MonoBehaviour
{
    private float defaultInterval = 0.2f;


    private Color flashColor = new Color(1f, 0.3f, 0.3f, 1f);

    public event Action OnFlashStart;
    public event Action OnFlashEnd;

    SpriteRenderer renderer2D;
    private Color originColor;
    private Coroutine isRunning;
    private bool isFlashing;

    public bool IsFlashing => isFlashing;


    private void Awake()
    {
        renderer2D = GetComponent<SpriteRenderer>();
        if(!renderer2D)
        {
            Debug.LogWarning($"{name} : FlashController에 SpriteRenderer가 없음");
            return;
        }
        originColor = renderer2D.color;
    }

    private void OnDisable() => StopFlash();


    public void StartFlash()
    {
        if (renderer2D == null) return;
        if (isRunning != null) StopCoroutine(isRunning);
        isRunning = StartCoroutine(FlashLoop());
    }

    public void StopFlash()
    {
        if (isRunning != null) 
        { 
            StopCoroutine(isRunning);
            isRunning = null; 
        }
        if (renderer2D != null) renderer2D.color = originColor;
    }

    private IEnumerator FlashLoop()
    {
        var wait = new WaitForSeconds(Mathf.Max(0.01f, defaultInterval));
        bool on = false;
        while (true)
        {
            renderer2D.color = on ? flashColor : originColor;
            on = !on;
            yield return wait;
        }
    }
}
