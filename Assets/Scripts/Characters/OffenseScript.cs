using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class OffenseScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject weaponRDamager;
    [SerializeField] private GameObject weaponLDamager;
    [SerializeField] private LayerMask enemyLayerMask;
    public bool debugSkipStep = false;
    [SerializeField] private List<AudioClip> sfxList;


    // References
    private Character character;
    private InputManager inputManager;
    private WeaponDial weaponDial;

    // Variables
    private float attackSector = 0f;

    private GameObject weaponDamager;

    private Vector3 damagerTopPos = new Vector3(0f, 1.3f, 0f);
    private Vector3 damagerBottomPos = new Vector3(0f, -0.7f, 0f);
    
    // Bools
    private bool ableToAttack = false;
    private bool isWeaponUpsideDown = false;

    // Comment - this limiter is added so that attacks don't perform
    //           animation cancelling at the start of another attack
    private bool attackSpamLimiterActive = false;

    private bool sfxLimiterActive = false;
    public float damage = 10f;

    private void Start()
    {
        character = GetComponent<Character>();
        inputManager = GetComponent<InputManager>();
        weaponDial = GetComponent<WeaponDial>();

        weaponDamager = weaponRDamager;
    }

    private void Update()
    {
        UpdatePossibleActions();

        if (!character.isDead)
            UpdateStatesAndAnimations();
    }

    #region MAIN

    private void UpdatePossibleActions()
    {
        // If any action is successfully performed, the limiter is deactivated
        if (character.isPerformingAnAction || character.isDead)
            attackSpamLimiterActive = false;
        
        // Can The Player Attack?
        if(character.isGrounded && !character.isPerformingAnAction && !attackSpamLimiterActive)
            ableToAttack = true;
        else
            ableToAttack = false;
    }

    private void UpdateStatesAndAnimations()
    {
        if(!character.isMovementRestriced || character.isStaggered)
            PerformResets();

        // Weapon Top Attack
        if (inputManager.tryingToWeaponTopAttack && ableToAttack)
            TopAttack();
        else
            inputManager.tryingToWeaponTopAttack = false;

        // Weapon Bottom Attack
        if (inputManager.tryingToWeaponBottomAttack && ableToAttack)
            BottomAttack();
        else
            inputManager.tryingToWeaponBottomAttack = false;

        // Weapon Thrust Attack
        if (inputManager.tryingToWeaponThrustAttack && ableToAttack)
            ThrustAttack();
        else
            inputManager.tryingToWeaponThrustAttack = false;

        // SFX
        if (character.isWeaponColliderActive && !sfxLimiterActive)
            PlaySFX();
        else if (!character.isWeaponColliderActive)
            sfxLimiterActive = false;

    }

    #endregion

    #region ATTACK

    private void TopAttack()
    {
        // Update States
        character.isUILocked = true;
        character.isMovementRestriced = true;
        attackSpamLimiterActive = true;

        // Clear Input
        inputManager.tryingToWeaponTopAttack = false;
        inputManager.bufferedAction = BufferActions.CLEAR;

        // Rotate the character to the target
        if (character.isLocking && character.target != null)
            LookAtTarget();

        // Perform a step
        if(!debugSkipStep)
        {
            StopAllCoroutines();
            if (GetNearEnemy())
                character.currentCoroutine = StartCoroutine(character.Step(true));
            else
                character.currentCoroutine = StartCoroutine(character.Step());
        }

        // Calculate sector to define animation - 8 sectors & 22.5 degree offset
        float thresholdAngle = weaponDial.topAngle + 22.5f > 360f ? (weaponDial.topAngle + 22.5f - 360f) / 45f : (weaponDial.topAngle + 22.5f) / 45f;
        attackSector = Mathf.Ceil(thresholdAngle);

        // Change weapon's hand, rotate and adjust collider
        UpdateWeapon(AttackType.SLASH_WEAPON_TOP, weaponDial.topAngle);

        // Set Animation
        character.animator.SetInteger(character.animKeys.comboKey, character.combo);
        character.animator.SetFloat(character.animKeys.attackDirection, attackSector);
        character.animator.SetTrigger(character.animKeys.attackTriggerKey);


        // Set character.combo
        if(character.combo == 0)
            character.SetAttackInfo(damage, weaponDial.topAngle, weaponDial.bottomAngle, AttackType.SLASH_WEAPON_TOP);
        else
            character.SetAttackInfo(damage * character.combo, weaponDial.topAngle, weaponDial.bottomAngle, AttackType.SLASH_WEAPON_TOP);

        character.combo = character.combo + 1 > 2 ? 0 : character.combo + 1;

        // Data Gather
        if (gameObject.tag == "Player")
            DataCollector.currentTest.performedTopAttacks += 1;
    }

    private void BottomAttack()
    {
        // Update States
        character.isMovementRestriced = true;
        character.isUILocked = true;
        attackSpamLimiterActive = true;

        // Clear Input
        inputManager.tryingToWeaponBottomAttack = false;
        inputManager.bufferedAction = BufferActions.CLEAR;

        // Rotate the character to the target
        if (character.isLocking && character.target != null)
            LookAtTarget();

        // Perform a step
        if (!debugSkipStep)
        {
            StopAllCoroutines();
            if (GetNearEnemy())
                character.currentCoroutine = StartCoroutine(character.Step(true));
            else
                character.currentCoroutine = StartCoroutine(character.Step());
        }

        // Calculate sector to define animation - 8 sectors & 22.5 degree offset
        float thresholdAngle = weaponDial.bottomAngle + 22.5f > 360f ? (weaponDial.bottomAngle + 22.5f - 360f) / 45f : (weaponDial.bottomAngle + 22.5f) / 45f;
        attackSector = Mathf.Ceil(thresholdAngle);

        // Change weapon's hand, rotate and adjust collider
        UpdateWeapon(AttackType.SLASH_WEAPON_BOTTOM, weaponDial.bottomAngle);

        // Set Animation
        character.animator.SetInteger(character.animKeys.comboKey, character.combo);
        character.animator.SetFloat(character.animKeys.attackDirection, attackSector);
        character.animator.SetTrigger(character.animKeys.attackTriggerKey);
        
        // Set character.combo
        if (character.combo == 0)
            character.SetAttackInfo(damage / 2f, weaponDial.topAngle, weaponDial.bottomAngle, AttackType.SLASH_WEAPON_BOTTOM);
        else
            character.SetAttackInfo(damage * character.combo / 2f, weaponDial.topAngle, weaponDial.bottomAngle, AttackType.SLASH_WEAPON_BOTTOM);

        character.combo = character.combo + 1 > 2 ? 0 : character.combo + 1;

        // Data Gather
        if (gameObject.tag == "Player")
            DataCollector.currentTest.performedBottomAttacks += 1;
    }

    private void ThrustAttack()
    {
        // Update States
        character.isMovementRestriced = true;
        character.isAttacking = true;
        character.isUILocked = false;
        attackSpamLimiterActive = true;

        // Clear Input
        inputManager.tryingToWeaponTopAttack = false;
        inputManager.bufferedAction = BufferActions.CLEAR;

        // Rotate the character to the target
        if (character.isLocking && character.target != null)
            LookAtTarget();

        // Perform a step
        if (!debugSkipStep)
        {
            character.doStep = false;
            character.doBackStep = false; 
            StopAllCoroutines();
            if (GetNearEnemy())
                character.currentCoroutine = StartCoroutine(character.Step(true));
            else
                character.currentCoroutine = StartCoroutine(character.Step());
        }

        // Assign Sector
        attackSector = 0f;

        // Change weapon's hand, rotate and adjust collider
        UpdateWeapon(AttackType.THRUST, 0f);

        // Set Animation
        character.animator.SetInteger(character.animKeys.comboKey, character.combo);
        character.animator.SetFloat(character.animKeys.attackDirection, attackSector);
        character.animator.SetTrigger(character.animKeys.attackTriggerKey);
        
        // Set character.combo
        if (character.combo == 0)
            character.SetAttackInfo(damage, weaponDial.topAngle, weaponDial.bottomAngle, AttackType.THRUST);
        else
            character.SetAttackInfo(damage * character.combo, weaponDial.topAngle, weaponDial.bottomAngle, AttackType.THRUST);

        character.combo = character.combo + 1 > 2 ? 0 : character.combo + 1;

        // Data Gather
        if (gameObject.tag == "Player")
            DataCollector.currentTest.performedThrustAttacks += 1;
    }


    #endregion

    #region WEAPON

    private void UpdateWeapon(AttackType type, float angle = 0f)
    {
        if (type != AttackType.THRUST)
        {
            // Hardcoded - As I didn't find light top/bottom attack animations, it will perform light high/low attacks
            if (character.combo < 2)
            {
                switch (attackSector)
                {
                    case 1:
                        attackSector = angle > 337.5f ? 8 : 2;
                        break;
                    case 5:
                        attackSector = angle > 180 ? 6 : 4;
                        break;
                    default:
                        break;
                }
            }

            // Switch between Left/Right hand depending on Left/Right part of the dial
            if (angle <= 180f)
                UpdateWeaponHand(false, true);
            else
                UpdateWeaponHand(true, false);

            // Hardcoded - For Light/Strong Mid Attack animations, the hand needs to be inversed for the character.combos to work
            switch (attackSector)
            {
                case 3:
                    UpdateWeaponHand(true, false);
                    break;
                case 7:
                    UpdateWeaponHand(false, true);
                    break;
                default:
                    break;
            }
        }
        else
            UpdateWeaponHand(false, true);

        // Rotate Weapon and Move Collider
        if (type != AttackType.SLASH_WEAPON_BOTTOM)
        {
            if (isWeaponUpsideDown)
                SwitchWeaponRotation();

            weaponDamager.transform.localPosition = damagerTopPos;
        }
        else
        {
            if (!isWeaponUpsideDown)
                SwitchWeaponRotation();

            weaponDamager.transform.localPosition = damagerBottomPos;
        }
    }

    private void UpdateWeaponHand(bool lActive, bool rActive)
    {
        character.UpdateWeapon(lActive, rActive);

        if (rActive)
            weaponDamager = weaponRDamager;
        else
            weaponDamager = weaponLDamager;
    }

    private void SwitchWeaponRotation()
    {
        if (isWeaponUpsideDown)
        {
            character.RWeapon.transform.Rotate(180f, 0f, 0f, Space.Self);
            character.LWeapon.transform.Rotate(0f, 0f, 180f, Space.Self);
            isWeaponUpsideDown = false;
        }
        else
        {
            character.RWeapon.transform.Rotate(180f, 0f, 0f, Space.Self);
            character.LWeapon.transform.Rotate(0f, 0f, 180f, Space.Self);
            isWeaponUpsideDown = true;
        }
    }

    #endregion

    #region HELPERS

    private void LookAtTarget()
    {
        // Y axis to 0 so Vector is calculated at same height
        Vector3 targetPos = new Vector3(character.target.transform.position.x, 0f, character.target.transform.position.z);
        Vector3 selfPos = new Vector3(transform.position.x, 0f, transform.position.z);
        transform.rotation = Quaternion.LookRotation(targetPos - selfPos);
    }

    private void PerformResets()
    {
        // Reset character.combo
        character.combo = 0;

        // Return Weapon to default
        if (isWeaponUpsideDown)
        {
            character.RWeapon.transform.Rotate(180f, 0f, 0f, Space.Self);
            character.LWeapon.transform.Rotate(0f, 0f, 180f, Space.Self);
            isWeaponUpsideDown = false;
        }

        // Reset Collider
        if (weaponDamager != null)
            DeactivateDamageCollider();
    }

    private bool GetNearEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3f, enemyLayerMask);

        bool ret = false;
        float distanceToCurrentTarget = 1000f;

        foreach (Collider collider in hitColliders)
        {
            if(character.target != null)
                distanceToCurrentTarget = (character.target.transform.position - transform.position).magnitude;

            if (collider.gameObject.tag == "Enemy")
            {
                float distanceToTarget = (collider.gameObject.transform.position - transform.position).magnitude;

                if (distanceToTarget <= distanceToCurrentTarget)
                {
                    character.target = collider.gameObject;
                    ret = true;
                }
            }
        }

        return ret;
    }

    private void ActivateDamageCollider()
    {
        character.isWeaponColliderActive = true;
        weaponDamager.SetActive(true);
    }

    private void DeactivateDamageCollider()
    {
        character.isWeaponColliderActive = false;
        character.ClearAttackInfo();
        weaponDamager.SetActive(false);
    }

    private void PlaySFX()
    {
        sfxLimiterActive = true;

        // Play SFX
        switch (character.attackInfo.type)
        {
            case AttackType.SLASH_WEAPON_TOP:
                character.audioSource.clip = sfxList[0];
                break;
            case AttackType.SLASH_WEAPON_BOTTOM:
                character.audioSource.clip = sfxList[1];
                break;
            case AttackType.THRUST:
                character.audioSource.clip = sfxList[2];
                break;
            case AttackType.NONE:
                return;
            default:
                break;
        }

        character.audioSource.pitch = character.combo - 1f < 0f ? 1.3f : 0.15f * character.combo + 0.85f;

        character.audioSource.Stop();
        character.audioSource.Play();
    }

    #endregion
}
