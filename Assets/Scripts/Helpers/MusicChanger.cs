using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicChanger : MonoBehaviour
{
    [SerializeField] private AudioClip music;
    private MusicManager musicManager;

    private void Start()
    {
        musicManager = gameObject.transform.root.gameObject.GetComponent<MusicManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            musicManager.SetMusic(music);
        }
    }
}
