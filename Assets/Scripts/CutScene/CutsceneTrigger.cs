using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class CutsceneTrigger : MonoBehaviour
{
    public PlayableDirector timelineDirector;

    public CinemachineCamera playerVCam;     // ���� ���� �÷��̾� ���󰡴� VCam
    public CinemachineCamera[] cutsceneVCams;

    private bool hasPlayed = false;

    public GameObject playerObject;                 // �÷��̾� ������Ʈ �Ҵ�

    private Vector2 originalVelocity;
    private Rigidbody2D playerRb;
    private Animator playerAnimator;
    RuntimeAnimatorController originalAnimator;

    private bool isPaused = false;
    private bool waitingForInput = false;

    public void DisablePlayerControl()
    {
        playerRb = playerObject.GetComponent<Rigidbody2D>();
        playerAnimator = playerObject.GetComponent<Animator>();

        if (playerRb != null)
        {
            originalVelocity = playerRb.linearVelocity;
            playerRb.linearVelocity = Vector2.zero;
            playerRb.simulated = false;
        }

        if (playerAnimator != null)
        {
            originalAnimator = playerAnimator.runtimeAnimatorController;
            playerAnimator.runtimeAnimatorController = null; // �ִϸ��̼� ���� ����
        }

        playerObject.GetComponent<Collider2D>().enabled = false;
    }

    public void EnablePlayerControl()
    {
        if (playerRb != null)
        {
            playerRb.simulated = true;
            playerRb.linearVelocity = originalVelocity;
        }

        if (playerAnimator != null && originalAnimator != null)
        {
            playerAnimator.runtimeAnimatorController = originalAnimator; // �ִϸ����� ����
        }

        playerObject.GetComponent<Collider2D>().enabled = true;
    }

    private void OnEnable()
    {
        timelineDirector.stopped += OnCutsceneEnd;
    }

    private void OnDisable()
    {
        timelineDirector.stopped -= OnCutsceneEnd;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasPlayed) return;

        if (other.CompareTag("Player"))
        {
            hasPlayed = true;
            DisablePlayerControl();

            // �ƾ� ī�޶� �켱���� ����
            foreach (var cam in cutsceneVCams)
                cam.Priority = 20;

            playerVCam.Priority = 10;

            timelineDirector.Play();
        }
    }

    private void OnCutsceneEnd(PlayableDirector director)
    {
        // �ƾ� ������ �÷��̾� ī�޶� ����
        playerVCam.Priority = 20;

        foreach (var cam in cutsceneVCams)
            cam.Priority = 10;
        EnablePlayerControl();
    }

    public void ForcePlay()
    {
        if (hasPlayed) return;

        hasPlayed = true;
        DisablePlayerControl();

        foreach (var cam in cutsceneVCams)
            cam.Priority = 20;

        playerVCam.Priority = 10;

        timelineDirector.Play();
    }

    // ======================�ƾ� ���߱�======================
    private Coroutine _startFreezeCoroutine;

    public void PauseTimelineExactly()
    {
        isPaused = true;
        timelineDirector.Pause();
        timelineDirector.time = timelineDirector.time;
        timelineDirector.Evaluate();
        _startFreezeCoroutine = StartCoroutine(FreezeTimeline());
    }

    public void ResumeTimeline()
    {
        isPaused = false;
        if (_startFreezeCoroutine != null)
            StopCoroutine(_startFreezeCoroutine);

        timelineDirector.Play();
    }

    private IEnumerator FreezeTimeline()
    {
        while (isPaused)
        {
            timelineDirector.time = timelineDirector.time;
            timelineDirector.Evaluate();
            yield return null;
        }
    }

    void Update()
    {
        if (isPaused && Input.GetKeyDown(KeyCode.F))
        {
            ResumeTimeline();
        }
    }
}
