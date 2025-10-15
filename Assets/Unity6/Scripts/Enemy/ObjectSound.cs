using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ObjectSound : MonoBehaviour
{
    [Header("사운드 설정")]
    [SerializeField]
    protected AudioClip hitClip;
    [SerializeField]
    protected AudioClip attackClip;
    [SerializeField]
    protected AudioClip deadClip;



    AudioSource audioSource;



    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    protected void PlaySound(AudioClip _clip)
    {
        audioSource.Stop();
        audioSource.clip = _clip;
        audioSource.Play();
    }

    public void HitSound()
    {
        PlaySound(hitClip);
    }

    public void DeadSound()
    {
        PlaySound(deadClip);
    }

    public void StopSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
