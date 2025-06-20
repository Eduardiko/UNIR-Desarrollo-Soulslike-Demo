using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : MonoBehaviour
{
    private Character character;
    [SerializeField] private float healSpeed = 1f;
    private void Start()
    {
        character = GetComponent<Character>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Heal")
        {
            if(character.health < character.maxHealth)
                character.health += healSpeed * Time.deltaTime;
        }
    }
}
