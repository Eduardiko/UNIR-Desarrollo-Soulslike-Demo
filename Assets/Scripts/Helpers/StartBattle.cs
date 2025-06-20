using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartBattle : MonoBehaviour
{

    public GameObject enemy1;
    public GameObject enemy2;

    private Character enemy1Character;
    private Character enemy2Character;

    private SmartEnemy enemy1Brain;
    private SmartEnemy enemy2Brain;

    private bool done = false;

    // Start is called before the first frame update
    void Start()
    {
        enemy1Character = enemy1.GetComponent<Character>();
        enemy2Character = enemy2.GetComponent<Character>();

        enemy1Brain = enemy1.GetComponent<SmartEnemy>();
        enemy2Brain = enemy2.GetComponent<SmartEnemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player" && !done)
            BeginBattle();
    }

    private void BeginBattle()
    {
        done = true;
        enemy1Character.target = enemy2;
        enemy2Character.target = enemy1;

        enemy1Brain.targetCharacter = enemy2Character;
        enemy2Brain.targetCharacter = enemy1Character;
    }
}
