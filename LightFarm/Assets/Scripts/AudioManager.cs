using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float musicVolume = 0.6f;

    [Header("SFX")]
    public AudioClip clickClip;
    public AudioClip placementClip;
    public AudioClip twinkleClip;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;
    [Range(0f, 0.5f)] public float sfxPitchVariance = 0.05f;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("UI")]
    public Image musicButtonImage;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    private bool musicEnabled = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupSources();
    }

    void Start()
    {
        PlayMusic();
    }

    void SetupSources()
    {
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    public void PlayMusic()
    {
        if (!musicEnabled || musicClip == null || musicSource == null)
        {
            return;
        }

        musicSource.clip = musicClip;
        musicSource.volume = musicVolume;
        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;

        if (musicEnabled)
        {
            PlayMusic();
        }
        else if (musicSource != null)
        {
            musicSource.Stop();
        }

        UpdateMusicIcon();
    }

    void UpdateMusicIcon()
    {
        if (musicButtonImage == null)
        {
            return;
        }

        if (musicEnabled)
        {
            if (musicOnSprite != null)
            {
                musicButtonImage.sprite = musicOnSprite;
            }
        }
        else
        {
            if (musicOffSprite != null)
            {
                musicButtonImage.sprite = musicOffSprite;
            }
        }
    }

    public void PlayClick()
    {
        PlaySfx(clickClip);
    }

    public void PlayPlacement()
    {
        PlaySfx(placementClip);
    }

    public void PlayTwinkle()
    {
        PlaySfx(twinkleClip);
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        float pitch = 1f + Random.Range(-sfxPitchVariance, sfxPitchVariance);
        sfxSource.pitch = pitch;
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(clip);
    }
}
