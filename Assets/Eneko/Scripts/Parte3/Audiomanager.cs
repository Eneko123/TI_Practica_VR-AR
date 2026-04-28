using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip gemCollectSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;
    [SerializeField] private AudioClip planeDetectedSound;
    [SerializeField] private AudioClip uiClickSound;

    [Header("Settings")]
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float sfxVolume = 1f;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Configurar AudioSources
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
            musicSource.loop = true;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null && !musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PlayGemSound()
    {
        PlaySound(gemCollectSound);
    }

    public void PlayVictorySound()
    {
        PlaySound(victorySound);
    }

    public void PlayDefeatSound()
    {
        PlaySound(defeatSound);
    }

    public void PlayPlaneDetectedSound()
    {
        PlaySound(planeDetectedSound);
    }

    public void PlayUIClickSound()
    {
        PlaySound(uiClickSound);
    }

    public void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayEndSound(bool victory)
    {
        if (victory)
            PlayVictorySound();
        else
            PlayDefeatSound();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public void ToggleMusic(bool enabled)
    {
        if (musicSource != null)
        {
            if (enabled)
                PlayBackgroundMusic();
            else
                StopBackgroundMusic();
        }
    }

    public void ToggleSFX(bool enabled)
    {
        if (sfxSource != null)
            sfxSource.mute = !enabled;
    }
}