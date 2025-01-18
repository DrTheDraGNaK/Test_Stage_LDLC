using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using Random = UnityEngine.Random;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    private Dictionary<SoundState, List<Sound>> DicoActualSound = new Dictionary<SoundState, List<Sound>>();

    [Header("Scene Settings")]
    [Dropdown("GetAvailableScenes")]
    [SerializeField] private string menuScene;

    [Dropdown("GetAvailableScenes")]
    [SerializeField] private string gameScene;


    private bool isMenuSoundPlayed = false;
    private bool isGameSoundPlayed = false;

    private Queue<Sound> musicQueue = new Queue<Sound>();
    private Sound currentPlayingSound;

    private bool isStoppedManually = false;

    public static AudioManager instance;

    private bool resetSound = false;

    private string currentSound;

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 10; // Taille du pool
    private Queue<AudioSource> audioSourcePool; // Pool d'AudioSource

    void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialisation du pool
        audioSourcePool = new Queue<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            //source.outputAudioMixerGroup = audioMain;
            audioSourcePool.Enqueue(source);
        }

        // Initialisation des sons
        foreach (Sound s in sounds)
        {
            if (!DicoActualSound.ContainsKey(s.ActualSound))
            {
                DicoActualSound.Add(s.ActualSound, new List<Sound>());
            }
            DicoActualSound[s.ActualSound].Add(s);

            if (s.AtStart)
                Play(s.Name);
        }
    }

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == menuScene && !isMenuSoundPlayed)
        {
            PlayRandom(SoundState.MENUMUSIC);
            if(isGameSoundPlayed)
            {
                StopFade(SoundState.GAMEMUSIC);
                Debug.Log("je retire la musique de jeu");
                isGameSoundPlayed = false;
            }
            isMenuSoundPlayed = true;
        }
        else if (currentScene == gameScene && !isGameSoundPlayed)
        {
            PlayRandom(SoundState.GAMEMUSIC);
            if (isMenuSoundPlayed)
            {
                StopFade(SoundState.MENUMUSIC);
                Debug.Log("je retire la musique de menu");
                isMenuSoundPlayed = false;

            }
            isGameSoundPlayed = true;
        }
        else if (currentScene != menuScene && currentScene != gameScene)
        {
            isMenuSoundPlayed = false;
            isGameSoundPlayed = false; // Reset if entering a different scene
        }
    }

    private AudioSource GetAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            return audioSourcePool.Dequeue();
        }

        Debug.LogWarning("Pool is empty. Creating a new AudioSource.");

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        return newSource;
    }

    private void ReturnAudioSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.loop = false;
        source.volume = 1f;
        source.pitch = 1f;
        audioSourcePool.Enqueue(source);
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.Name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return;
        }

        AudioSource source = GetAudioSource();
        if (source == null) return;

        s.Source = source;
        s.Source.clip = s.Clip;
        s.Source.volume = s.Volume;
        s.Source.pitch = s.Pitch;
        s.Source.loop = s.Loop;
        s.Source.outputAudioMixerGroup = s.AudioMixer;

        s.Source.Play();

        if (!s.Loop)
        {
            StartCoroutine(ReturnSourceToPoolAfterPlay(s.Source, s.Source.clip.length));
        }
    }

    public void Stop(SoundState soundState)
    {
        if (DicoActualSound.ContainsKey(soundState))
        {
            foreach (Sound s in DicoActualSound[soundState])
            {
                if (s.Source != null && s.Source.isPlaying)
                {
                    s.Source.Stop();
                    ReturnAudioSource(s.Source);
                }
            }
        }
        else
        {
            Debug.LogWarning("No sounds found for SoundState: " + soundState);
        }
    }

    public void PlayRandom(SoundState soundState)
    {
        if (DicoActualSound.ContainsKey(soundState))
        {
            int i = Random.Range(0, DicoActualSound[soundState].Count);
            Sound s = DicoActualSound[soundState][i];
            if (s != null)
            {
                Play(s.Name);
            }
        }
        else
        {
            Debug.LogWarning("SoundState not found: " + soundState);
        }
    }

    public void PlayMusicByState(SoundState soundState)
    {
        if(currentSound != null)
        {
            StopCurrentMusic();

        }

        isStoppedManually = false;
        GenerateRandomMusicQueue(soundState);
        PlayNextInQueue();
    }

    public void GenerateRandomMusicQueue(SoundState soundState)
    {
        if (!DicoActualSound.ContainsKey(soundState))
        {
            Debug.LogWarning("No sounds found for SoundState: " + soundState);
            return;
        }

        List<Sound> availableSounds = new List<Sound>(DicoActualSound[soundState]);
        musicQueue.Clear();

        while (availableSounds.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSounds.Count);
            musicQueue.Enqueue(availableSounds[randomIndex]);
            availableSounds.RemoveAt(randomIndex);
        }
    }

    public void PlayNextInQueue()
    {
        if (isStoppedManually || musicQueue.Count == 0)
        {
            return;
        }

        currentPlayingSound = musicQueue.Dequeue();
        Play(currentPlayingSound.Name);

        StartCoroutine(WaitForMusicToEnd(currentPlayingSound.Source.clip.length));
    }

    public void StopCurrentMusic()
    {
        if (currentPlayingSound != null && currentPlayingSound.Source.isPlaying)
        {
            currentPlayingSound.Source.Stop();
            ReturnAudioSource(currentPlayingSound.Source);
        }
        isStoppedManually = true;
    }

    IEnumerator WaitForMusicToEnd(float duration)
    {
        yield return new WaitForSeconds(duration);
        PlayNextInQueue();
    }

    IEnumerator ReturnSourceToPoolAfterPlay(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        ReturnAudioSource(source);
    }

    public void PlayFade(SoundState soundState)
    {
        StopFade(soundState);
        if (DicoActualSound.ContainsKey(soundState))
        {
            int i = Random.Range(0, DicoActualSound[soundState].Count);
            Sound s = DicoActualSound[soundState][i];
            if (s != null)
            {
                StartCoroutine(FadeIn(s));
            }
        }
    }

    public void StopFade(SoundState soundState)
    {
        if (DicoActualSound.ContainsKey(soundState))
        {
            foreach (Sound s in DicoActualSound[soundState])
            {
                if (s.Source != null && s.Source.isPlaying)
                {
                    StartCoroutine(FadeOut(s));
                }
            }
        }
    }

    public IEnumerator FadeOut(Sound s)
    {
        if (s.Source != null)
        {
            s.Source.DOFade(0f, 2f);
            yield return new WaitForSeconds(2f);
            ReturnAudioSource(s.Source);
        }
    }

    public IEnumerator FadeIn(Sound s)
    {
        AudioSource source = GetAudioSource();
        if (source == null) yield break;

        s.Source = source;
        s.Source.clip = s.Clip;
        s.Source.volume = 0f;
        s.Source.Play();
        s.Source.DOFade(s.Volume, 2f);

        if (!s.Loop)
        {
            StartCoroutine(ReturnSourceToPoolAfterPlay(s.Source, s.Source.clip.length));
        }

        yield return null;
    }

    // Nouvelle fonction Pause
    public void Pause(SoundState soundState)
    {
        if (DicoActualSound.ContainsKey(soundState))
        {
            foreach (Sound s in DicoActualSound[soundState])
            {
                if (s.Source != null && s.Source.isPlaying)
                {
                    s.Source.Pause();
                }
            }
        }
        else
        {
            Debug.LogWarning("No sounds found for SoundState: " + soundState);
        }
    }

    // Nouvelle fonction UnPause
    public void UnPause(SoundState soundState)
    {
        if (DicoActualSound.ContainsKey(soundState))
        {
            foreach (Sound s in DicoActualSound[soundState])
            {
                if (s.Source != null)
                {
                    s.Source.UnPause();
                }
            }
        }
        else
        {
            Debug.LogWarning("No sounds found for SoundState: " + soundState);
        }
    }

    private List<string> GetAvailableScenes()
    {
        List<string> scenes = new List<string>();
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            scenes.Add(name);
        }
        return scenes;
    }
}
