using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagableScript : MonoBehaviour
{
    [SerializeField] private List<AudioClip> sfxList;
    [SerializeField] private List<float> protectedAngles;

    // References
    private Character character;
    private WeaponDial weaponDial;

    // Variables
    private Character attackerCharacter;

    private float damageResetTime = 0.3f;
    private float damageResetTimer = 0f;

    private float guardThresholdAngle = 45f;
    private float parryThresholdAngle = 10f;

    private void Start()
    {
        character = GetComponentInChildren<Character>();
        weaponDial = GetComponent<WeaponDial>();

        damageResetTimer = damageResetTime;
    }

    private void Update()
    {
        // When taken damage, give invincibility for a short ammount of time
        if(damageResetTimer < damageResetTime)
            damageResetTimer += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Weapon" && other.transform.root.gameObject != gameObject)
        {
            if (!character.isImmuneToDamage && !character.isDead && damageResetTimer >= damageResetTime)
            {
                attackerCharacter = other.transform.root.gameObject.GetComponent<Character>();

                ManageDamage();

                // Resets
                damageResetTimer = 0f;
            }
        }
    }

    #region MAIN
    
    public void ManageDamage()
    {
        // Check if any of the two Weapon Angles is inside the threshold
        float angularDifference;

        if (attackerCharacter.attackInfo.type == AttackType.SLASH_WEAPON_TOP)
        {
            foreach (float protectedAngle in protectedAngles)
            {
                angularDifference = Mathf.Abs(Mathf.DeltaAngle(protectedAngle, attackerCharacter.attackInfo.topAngle));

                // Depending on the angular difference, choose what to apply
                if (angularDifference < parryThresholdAngle)
                {
                    Parry(false);
                    return;
                }
            }
        }

        if (attackerCharacter.attackInfo.type == AttackType.SLASH_WEAPON_TOP)
            angularDifference = Mathf.Abs(Mathf.DeltaAngle(weaponDial.topAngle, 360 - attackerCharacter.attackInfo.topAngle));
        else
            angularDifference = Mathf.Abs(Mathf.DeltaAngle(weaponDial.topAngle, 360 - attackerCharacter.attackInfo.bottomAngle));

        // Depending on the angular difference, choose what to apply
        if (angularDifference > guardThresholdAngle || attackerCharacter.attackInfo.type == AttackType.THRUST || character.isMovementRestriced)
            Hit();
        else if (angularDifference < parryThresholdAngle && !character.isMovementRestriced)
            Parry();
        else if (!character.isMovementRestriced)
            Guard(angularDifference);
    }

    #endregion

    #region ACTIONS
    public void Hit()
    {
        // Trigger Counter Parry to attacker
        if (character.isAttacking && character.attackInfo.type == AttackType.THRUST && attackerCharacter.attackInfo.type == AttackType.THRUST)
        {
            attackerCharacter.animator.SetFloat(attackerCharacter.animKeys.hitID, 10f);
            attackerCharacter.animator.SetTrigger(attackerCharacter.animKeys.hitTriggerKey);

            // Data Gather
            if (gameObject.tag == "Player")
                DataCollector.currentTest.successfulCounterThrusts += 1;

            return;
        }

        // Apply Damage
        character.health -= attackerCharacter.attackInfo.damageAmmount;

        // Data Gather
        if (gameObject.tag == "Player")
            DataCollector.currentTest.hitsReceived += 1;

        // Check Death
        if (character.health <= 0f)
        {
            Die();
            return;
        }

        // Set States
        character.isUILocked = false;

        // Play SFX
        character.audioSource.clip = sfxList[0];
        float randomPitch = Random.Range(0.85f, 1.15f);
        character.audioSource.pitch = randomPitch;
        character.audioSource.Stop();
        character.audioSource.Play();

        // Apply/Not apply staggered animation
        if (!character.isAttacking)
        {
            character.isMovementRestriced = true;
            character.isStaggered = true;

            float randomAnimID = Random.Range(1f, 5f);
            character.animator.SetFloat(character.animKeys.hitID, randomAnimID);
            character.animator.SetTrigger(character.animKeys.hitTriggerKey);
        }
    }

    public void Guard(float angularDifference)
    {
        // Calculate damage received based on accuracy && apply damage
        float mitigationMultiplier = parryThresholdAngle / angularDifference;
        float mitigationAmmount = 4f * mitigationMultiplier * mitigationMultiplier;
        float damage = attackerCharacter.attackInfo.damageAmmount / (1f + mitigationAmmount);
        character.health -= damage;

        // Data Gather
        if (gameObject.tag == "Player")
            DataCollector.currentTest.successfulGuards += 1;

        // Check Death
        if (character.health <= 0f)
        {
            Die();
            return;
        }

        // Set States
        character.isMovementRestriced = true;
        character.isStaggered = true;
        character.isUILocked = false;

        // Update UI
        weaponDial.SetGuardSprites();
        character.isStaggered = true;

        // Play SFX
        character.audioSource.pitch = 2f - angularDifference / guardThresholdAngle;
        character.audioSource.clip = sfxList[3];
        character.audioSource.Stop();
        character.audioSource.Play();

        // Apply/Not apply staggered animation
        if (!character.isPerformingAnAction)
        {
            character.animator.SetTrigger(character.animKeys.hitTriggerKey);
            float randomAnimID = Random.Range(6f, 8f);
            character.animator.SetFloat(character.animKeys.hitID, randomAnimID);
        }
    }

    public void Parry(bool triggerCounteranimation = true)
    {
        // Parry animation
        if(triggerCounteranimation)
        {
            // Set States
            character.isMovementRestriced = true;
            character.isStaggered = true;
            character.isUILocked = false;

            // Set Parry Animation
            character.animator.SetFloat(character.animKeys.hitID, 9f);
            character.animator.SetTrigger(character.animKeys.hitTriggerKey);

            // Play SFX
            character.audioSource.pitch = 1f;
            character.audioSource.clip = sfxList[1];
            character.audioSource.Stop();
            character.audioSource.Play();

            // Data Gather
            if (gameObject.tag == "Player")
                DataCollector.currentTest.successfulParries += 1;
        }

        if (!triggerCounteranimation)
        {
            // Play SFX
            character.audioSource.pitch = 1f;
            character.audioSource.clip = sfxList[2];
            character.audioSource.Stop();
            character.audioSource.Play();
        }
            

        // Update UI
        WeaponDial attackerWeaponDial = attackerCharacter.gameObject.GetComponent<WeaponDial>();
        attackerWeaponDial.SetCancelledSprites();

        // Set States
        attackerCharacter.isMovementRestriced = true;
        attackerCharacter.isStaggered = true;
        attackerCharacter.isUILocked = false;

        // Trigger Parry Hit to attacker
        attackerCharacter.animator.SetFloat(attackerCharacter.animKeys.hitID, 10f);
        attackerCharacter.animator.SetTrigger(attackerCharacter.animKeys.hitTriggerKey);

        // Data Gather
        if (attackerCharacter.gameObject.tag == "Player")
        {
            if(triggerCounteranimation)
                DataCollector.currentTest.failedAttacksbyTopWeapon += 1;
            else
                DataCollector.currentTest.failedAttacksByProtectedArea += 1;
        }
    }

    public void Die()
    {
        // Make attacker stop locking
        attackerCharacter.isLocking = false;

        // Update UI
        weaponDial.SetCancelledSprites();

        // Set States
        character.isLocking = false;
        character.isDead = true;

        // Play SFX
        character.audioSource.clip = sfxList[4];
        float randomPitch = Random.Range(0.85f, 1.15f);
        character.audioSource.pitch = randomPitch;
        character.audioSource.Stop();
        character.audioSource.Play();

        // Trigger Death animation
        float randomAnimID = Mathf.Round(Random.Range(11f, 12f));
        character.animator.SetFloat(character.animKeys.hitID, randomAnimID);
        character.animator.SetTrigger(character.animKeys.hitTriggerKey);

        // Data Gather
        if (gameObject.tag == "Player")
            DataCollector.currentTest.deathsCount += 1;

        // Update tag & layer
        gameObject.tag = "Corpse";
        gameObject.layer = 8;
    }

    #endregion
}
