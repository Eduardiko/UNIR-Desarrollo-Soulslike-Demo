using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAmbienceMusic : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!audioSource.isPlaying)
            audioSource.Play();
    }
}
