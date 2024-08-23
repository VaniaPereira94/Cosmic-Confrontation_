using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
   
    private AudioSource[] allAudioSources;

    private void Awake()
    {

        Instance = this;
        allAudioSources = FindObjectsOfType<AudioSource>();
   }
    // Start is called before the first frame update
  

    // Update is called once per frame
    public void PauseAll()
    {
        foreach(AudioSource audioSource in allAudioSources)
        {
            audioSource.Pause();
        }
    }

    public void ResumeAll()
    {
        foreach (AudioSource audioSource in allAudioSources)
        {
            audioSource.UnPause();
        }
    }
}
