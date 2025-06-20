using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioClip currentClip;

    private void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
    }
    public void SetMusic(AudioClip musicClip)
    {
        if(currentClip != musicClip)
        {
            currentClip = musicClip;
            audioSource.clip = musicClip;
            audioSource.Play();
        }
    }
}
