using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip[] gameplayMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGameplayMusic()
    {
        if (musicSource != null && gameplayMusic != null && !musicSource.isPlaying)
        {
            musicSource.clip = gameplayMusic[0];
            musicSource.Play();
            Debug.Log("Gameplay music started");
        }
    }
}