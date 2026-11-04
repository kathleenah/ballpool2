using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip collectSfx;
    public AudioClip badSfx;
    public AudioClip winSfx;
    public AudioClip splashSfx;

    public AudioSource bgMusic;

    [Range(0f, 1f)] public float badVolume = 0.3f;

    private AudioSource source;

    void Awake(){ source = GetComponent<AudioSource>(); }

    private void Play(AudioClip clip, float volume = 1f)
    {
        if (clip != null) source.PlayOneShot(clip, volume);
    }

    public void StopMusic()
    {
        if (bgMusic != null) bgMusic.Stop();
    }

    public void PlayCollect()
    { 
        Play(collectSfx); 
    }
    public void PlayBad()     
    { 
        Play(badSfx, badVolume); 
    }
    public void PlaySplash()
    {
        Play(splashSfx);
    }
    public void PlayWin()
    {
        if (bgMusic != null) bgMusic.volume = 0.05f;
        Play(winSfx);
    }
}