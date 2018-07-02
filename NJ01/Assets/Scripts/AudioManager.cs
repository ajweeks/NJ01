using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour 
{
    public static AudioManager instance = null;

    // This number should equal the most number of sounds that can ever play concurrently
    public int MaxSourceCount = 8;

    private AudioSource[] SFXSources;
    private AudioSource MusicSource;

    public AudioClip[] Clips;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            SFXSources = new AudioSource[MaxSourceCount];
            for (int i = 0; i < SFXSources.Length; ++i)
            {
                SFXSources[i] = gameObject.AddComponent<AudioSource>();
                SFXSources[i].playOnAwake = false;
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
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
