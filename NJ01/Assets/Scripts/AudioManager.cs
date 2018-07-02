using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour 
{
    public static AudioManager Instance = null;

    // This number should equal the most number of sounds that can ever play concurrently
    public int MaxSourceCount = 8;

    private AudioSource[] SFXSources;
    private AudioSource MusicSource;

    public AudioClip[] Songs;
    public AudioClip[] Clips;

    private float _masterVolume = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SFXSources = new AudioSource[MaxSourceCount];
            for (int i = 0; i < SFXSources.Length; ++i)
            {
                SFXSources[i] = gameObject.AddComponent<AudioSource>();
                SFXSources[i].playOnAwake = false;
            }

            MusicSource = gameObject.AddComponent<AudioSource>();
            MusicSource.playOnAwake = false;
            MusicSource.loop = true;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (_masterVolume == 0.0f)
            {
                SetMasterVolume(1.0f);
            }
            else
            {
                SetMasterVolume(0.0f);
            }
        }
    }

    public void PlaySong(string name)
    {
        for (int i = 0; i < Songs.Length; ++i)
        {
            if (Songs[i].name.CompareTo(name) == 0)
            {
                MusicSource.clip = Songs[i];
                MusicSource.Play();
                return;
            }
        }

        Debug.LogError("Failed to find song named " + name);
    }

    public void PlaySound(string name)
    {
        for (int i = 0; i < Clips.Length; ++i)
        {
            if (Clips[i].name.CompareTo(name) == 0)
            {
                int sourceIndex = GetNextAvailableSFXSource();
                SFXSources[sourceIndex].clip = Clips[i];
                SFXSources[sourceIndex].Play();
                SFXSources[sourceIndex].volume = _masterVolume;
                return;
            }
        }

        Debug.LogError("Failed to find sound named " + name);
    }

    public void PlaySoundRandomized(string nameStart)
    {
        List<int> soundIndices = new List<int>();

        for (int i = 0; i < Clips.Length; ++i)
        {
            if (Clips[i].name.StartsWith(nameStart))
            {
                soundIndices.Add(i);
            }
        }

        if (soundIndices.Count == 0)
        {
            Debug.LogError("Failed to find matching randomized sounds that start with " + nameStart);
        }
        else
        {
            int soundIndex = Random.Range(0, soundIndices.Count);
            int sourceIndex = GetNextAvailableSFXSource();
            SFXSources[sourceIndex].clip = Clips[soundIndices[soundIndex]];
            SFXSources[sourceIndex].Play();
        }
    }

    public void SetMasterVolume(float masterVolume)
    {
        _masterVolume = masterVolume;

        for (int i = 0; i < SFXSources.Length; ++i)
        {
            SFXSources[i].volume = _masterVolume;
        }

        MusicSource.volume = _masterVolume;
    }

    private int GetNextAvailableSFXSource()
    {
        for (int i = 0; i < SFXSources.Length; ++i)
        {
            if (!SFXSources[i].isPlaying)
            {
                return i;
            }
        }
        return 0;
    }
}
