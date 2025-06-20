using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private MusicManager musicManager;
    [SerializeField] private AudioClip windClip;

    private Transform spawnPoint;
    private Character character;

    private float dieTime = 3f;

    private void Start()
    {
        character = GetComponent<Character>();
    }

    private void FixedUpdate()
    {
        if(Input.GetKeyDown(KeyCode.F10))
            Application.Quit();

        if(character.isDead)
        {
            if (dieTime > 0f)
            {
                character.isPerformingAnAction = true;
                dieTime -= Time.deltaTime;
                return;
            }

            dieTime = 3f;

            gameObject.tag = "Player";
            gameObject.layer = 0;

            musicManager.SetMusic(windClip);

            character.health = character.maxHealth;
            transform.position = new Vector3(spawnPoint.position.x, transform.position.y, spawnPoint.position.z);
            transform.rotation = spawnPoint.rotation;
            character.ResetStates();
            character.isDead = false;
            character.animator.SetFloat(character.animKeys.hitID, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Checkpoint")
            spawnPoint = other.transform;
    }

}
