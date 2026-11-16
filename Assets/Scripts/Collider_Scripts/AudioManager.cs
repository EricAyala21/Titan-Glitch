
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Level Music")]
    [SerializeField] private AudioClip[] levelSongs;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    [Header("Random Music Delay (Seconds)")]
    [SerializeField] private float minMusicDelay = 60f;
    [SerializeField] private float maxMusicDelay = 180f;

    private AudioSource musicSource;
    private AudioClip lastPlayedSong;

    private Coroutine musicCoroutine;

    private void Awake()
    {
        // Prevent duplicate AudioManagers.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep AudioManager alive when scenes reload.
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();

        // Music should not start automatically.
        musicSource.playOnAwake = false;

        // Music should never loop.
        musicSource.loop = false;

        musicSource.volume = musicVolume;

        // Background music should be 2D.
        musicSource.spatialBlend = 0f;
    }

    private void Start()
    {
        // Play music when the game first starts.
        PlayRandomLevelSong();
    }

    public void PlayRandomLevelSong()
    {
        // Stop the previous song and timer.
        StopMusic();

        if (levelSongs == null || levelSongs.Length == 0)
        {
            Debug.LogWarning(
                "AudioManager: No songs assigned!"
            );
            return;
        }

        List<AudioClip> availableSongs =
            new List<AudioClip>();

        // Exclude the previous song.
        foreach (AudioClip song in levelSongs)
        {
            if (song != null && song != lastPlayedSong)
            {
                availableSongs.Add(song);
            }
        }

        // If there is only one valid song,
        // allow the same song to play again.
        if (availableSongs.Count == 0)
        {
            foreach (AudioClip song in levelSongs)
            {
                if (song != null)
                {
                    availableSongs.Add(song);
                }
            }
        }

        if (availableSongs.Count == 0)
        {
            Debug.LogWarning(
                "AudioManager: No valid audio clips!"
            );
            return;
        }

        // Select a random song.
        int randomIndex = Random.Range(
            0,
            availableSongs.Count
        );

        AudioClip selectedSong =
            availableSongs[randomIndex];

        // Assign the selected song.
        musicSource.clip = selectedSong;

        // Ensure music does not loop.
        musicSource.loop = false;

        // Begin playing.
        musicSource.Play();

        lastPlayedSong = selectedSong;

        Debug.Log(
            "Now Playing: " + selectedSong.name
        );

        // Start waiting for the song to finish.
        musicCoroutine =
            StartCoroutine(WaitForNextSong());
    }

    private IEnumerator WaitForNextSong()
    {
        // Allow audio playback to initialize.
        yield return null;

        // Wait until the current song finishes.
        yield return new WaitUntil(
            () => !musicSource.isPlaying
        );

        Debug.Log("Music Finished");

        // Calculate a random delay between
        // 1 and 3 minutes.
        float minDelay = Mathf.Max(
            0f,
            minMusicDelay
        );

        float maxDelay = Mathf.Max(
            minDelay,
            maxMusicDelay
        );

        float randomDelay = Random.Range(
            minDelay,
            maxDelay
        );

        Debug.Log(
            "Next song in " +
            randomDelay.ToString("F1") +
            " seconds"
        );

        // Wait in silence.
        yield return new WaitForSeconds(randomDelay);

        musicCoroutine = null;

        // Play another random song.
        PlayRandomLevelSong();
    }

    public void StopMusic()
    {
        // Stop the music coroutine.
        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
            musicCoroutine = null;
        }

        // Stop the current song.
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    private void OnDestroy()
    {
        // Clear the Singleton if this is the
        // active AudioManager being destroyed.
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
