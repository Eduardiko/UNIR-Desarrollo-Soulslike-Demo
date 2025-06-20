using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    
    [SerializeField] private Character player;
    private Character target;

    [SerializeField] private GameObject targetUI;

    [SerializeField] private Image playerHealthBar;
    [SerializeField] private Image targetHealthBar;
    [SerializeField] private TMP_Text targetName;


    // Update is called once per frame
    void Update()
    {
       if(player.isLocking && player.target != null)
        {
            target = player.target.GetComponent<Character>();

            targetName.text = target.gameObject.name;

            targetUI.SetActive(true);
        }
       else
            targetUI.SetActive(false);

        UpdateHealths();
    }

    private void UpdateHealths()
    {
        playerHealthBar.fillAmount = player.health / player.maxHealth;

        if(targetUI.activeSelf)
            targetHealthBar.fillAmount = target.health / target.maxHealth;

    }
}
