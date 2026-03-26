using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip placePieceClip;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.6f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureAudioSources();
        PlayBgmIfNeeded();
    }

    public void PlayBgmIfNeeded()
    {
        if (bgmSource == null || bgmClip == null)
        {
            return;
        }

        if (bgmSource.isPlaying && bgmSource.clip == bgmClip)
        {
            return;
        }

        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickClip);
    }

    public void PlayPlacePiece()
    {
        PlaySfx(placePieceClip);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
    }
}
