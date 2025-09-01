using UnityEngine;

//효과음 관련 코드
public class SFXSound : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip _clip)
    {
        if (audioSource == null) return;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = _clip;
            audioSource.Play();
        }
    }

}
