    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartEnemy : MonoBehaviour
{
    // References
    public GameObject player;
    private Character character;
    private InputManager inputManager;
    private WeaponDial weaponDial;
    private CharacterController characterController;
    [HideInInspector] public Character targetCharacter;

    // Variables
    [SerializeField] private float actionAttackMaxTriggerTime = 5f;
    [SerializeField] private float actionAttackMinTriggerTime = 5f;
    private float actionAttackTriggerTime = 1f;

    [Range(0.0f, 100.0f)]
    [SerializeField] private float thrustAttackProbability = 5f;
    private float thrustAttackProbabilityMultiplier;

    [Range(0.0f, 100.0f)]
    [SerializeField] private float bottomAttackProbability = 5f;

    [Range(0.0f, 100.0f)]
    [SerializeField] private float evadeProbability = 5f;
    private float evadeProbabilityMultiplier;
    private bool triedEvading = false;

    [Range(0.0f, 100.0f)]
    [SerializeField] private float guardProbability = 5f;
    [Range(0.0f, 100f)]
    [SerializeField] private float guardAccuracy = 5f;
    private float guardProbabilityMultiplier;
    private bool triedGuarding = false;

    [Range(0.0f, 100f)]
    [SerializeField] private float parryProbability = 5f;
    private float parryProbabilityMultiplier;
    private bool triedParrying = false;

    [Range(0.0f, 100.0f)]
    [SerializeField] private float counterThrustProbability = 5f;

    [Range(0.0f, 100.0f)]
    [SerializeField] private float backstepProbability = 5f;

    public float forcedAngle = 0f;

    public float moveSpeed = 0.5f;

    private Vector3 initialPosition;

    private Vector3 toTargetVector;
    private Vector3 toInitVector;

    private bool manualNotLookAtActive = false;

    private bool hasPerformedDefensiveAction = false;
    private bool hasSetProbabilities = false;

    public bool probabilitiesBasedOnVarietyOfAttacks = false;
    private float varietyMultiplier = 0f;
    private float comboMultiplier = 0f;
    private int lastCombo = 0;

    private AttackType lastReceivedAttackType;

    private int moveDirection = 1;

    private void Start()
    {
        inputManager = GetComponent<InputManager>();
        weaponDial = GetComponent<WeaponDial>();
        character = GetComponent<Character>();
        characterController = GetComponent<CharacterController>();

        character.target = player;
        targetCharacter = player.GetComponent<Character>();

        initialPosition = transform.position;

        lastReceivedAttackType = AttackType.NONE;
        
        actionAttackTriggerTime = actionAttackMaxTriggerTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetCharacter.isDead)
        {
            gameObject.transform.position = initialPosition;
            character.health = character.maxHealth;
            character.ResetStates();
        }

        if (!character.isDead && !targetCharacter.isDead)
        {
            if (!character.isMovementRestriced)
                MoveToTarget();

            // Animation Mode
            if (character.isLocking)
                character.animator.SetBool(character.animKeys.isLockingKey, true);
            else
                character.animator.SetBool(character.animKeys.isLockingKey, false);

            if (character.isLocking)
            {
                ManageProbabilities();

                Attack();

                Parry();
                Dodge();
                Guard();

                if (!targetCharacter.isWeaponColliderActive)
                {
                    hasPerformedDefensiveAction = false;
                    weaponDial.isUIWeaponAttached = false;
                    triedEvading = false;
                    triedGuarding = false;
                    triedParrying = false;
                }

                if (!character.isWeaponColliderActive || manualNotLookAtActive)
                {
                    // This way it stops looking at the player from the moment the collider is active until the end of the action, giving a sense of locked attack
                    if (character.isMovementRestriced && manualNotLookAtActive)
                        return;
                    else if (manualNotLookAtActive)
                        manualNotLookAtActive = false;

                    // Rotate the player to face the target
                    // Y axis to 0 so Vector is calculated at same height
                    Vector3 targetPos = new Vector3(character.target.transform.position.x, 0f, character.target.transform.position.z);
                    Vector3 selfPos = new Vector3(transform.position.x, 0f, transform.position.z);
                    transform.rotation = Quaternion.LookRotation(targetPos - selfPos);
                }
                else if (character.isWeaponColliderActive && !manualNotLookAtActive)
                    manualNotLookAtActive = true;
            }
        }
    }

    private void ManageProbabilities()
    {
        if (!probabilitiesBasedOnVarietyOfAttacks)
        {
            comboMultiplier = targetCharacter.combo - 1f < 0f ? 2f : targetCharacter.combo - 1f;

            SetProbabilities(comboMultiplier);
        }
        else
        {
            if (!hasSetProbabilities && targetCharacter.isAttacking)
            {
                hasSetProbabilities = true;

                lastCombo = targetCharacter.combo;

                if (targetCharacter.attackInfo.type == lastReceivedAttackType)
                    varietyMultiplier += 1f;
                else
                    varietyMultiplier = 0f;

                SetProbabilities(varietyMultiplier);

                lastReceivedAttackType = targetCharacter.attackInfo.type;

            }
            else if(!targetCharacter.isAttacking || (targetCharacter.isAttacking && lastCombo != targetCharacter.combo))
                hasSetProbabilities = false;
        }
        
    }

    private void MoveToTarget()
    {
        toTargetVector = character.target.transform.position - transform.position;
        toInitVector = initialPosition - transform.position;

        if (toTargetVector.magnitude > 5f && toTargetVector.magnitude < 10f)
        {
            character.isLocking = false;
            toTargetVector = toTargetVector.normalized * moveSpeed * Time.deltaTime;
            character.animator.SetFloat(character.animKeys.directionZKey, 1f);
            characterController.Move(toTargetVector);

            // Rotate the player to face the target
            // Y axis to 0 so Vector is calculated at same height
            Vector3 targetPos = new Vector3(character.target.transform.position.x, 0f, character.target.transform.position.z);
            Vector3 selfPos = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.rotation = Quaternion.LookRotation(targetPos - selfPos);

            actionAttackTriggerTime = 1f;
        }
        else if (toTargetVector.magnitude >= 10f && toInitVector.magnitude > 0.2f)
        {
            character.isLocking = false;
            toInitVector = toInitVector.normalized * moveSpeed * Time.deltaTime;
            character.animator.SetFloat(character.animKeys.directionZKey, 1f);
            characterController.Move(toInitVector);

            Vector3 targetPos = new Vector3(initialPosition.x, 0f, initialPosition.z);
            Vector3 selfPos = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.rotation = Quaternion.LookRotation(targetPos - selfPos);
        }
        else if(toTargetVector.magnitude <= 7.5f && toTargetVector.magnitude > 2f)
        {
            character.isLocking = true;
            toTargetVector = toTargetVector.normalized * moveSpeed /10f * Time.deltaTime;
            character.animator.SetFloat(character.animKeys.directionZKey, 0.4f);
            characterController.Move(toTargetVector);

            Vector3 targetPos = new Vector3(character.target.transform.position.x, 0f, character.target.transform.position.z);
            Vector3 selfPos = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.rotation = Quaternion.LookRotation(targetPos - selfPos);
        }
        else if(toTargetVector.magnitude <= 5f)
        {
            Vector3 direction = gameObject.transform.right * moveSpeed * moveDirection / 10f * Time.deltaTime;
            character.animator.SetFloat(character.animKeys.directionZKey, 0.2f);
            characterController.Move(direction);

            character.isLocking = true;

            Vector3 targetPos = new Vector3(character.target.transform.position.x, 0f, character.target.transform.position.z);
            Vector3 selfPos = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.rotation = Quaternion.LookRotation(targetPos - selfPos);
        } else
        {
            character.isLocking = false;
            character.animator.SetFloat(character.animKeys.directionZKey, 0f);

            Vector3 targetPos = new Vector3(character.target.transform.position.x, 0f, character.target.transform.position.z);
            Vector3 selfPos = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.rotation = Quaternion.LookRotation(targetPos - selfPos);
        }
    }

    private void Attack()
    {
        if (actionAttackTriggerTime <= 0f)
        {
            actionAttackTriggerTime = Random.Range(actionAttackMinTriggerTime / (1f + character.combo), actionAttackMaxTriggerTime);

            float randomType = Random.Range(0f, 100f);
            if (randomType <= thrustAttackProbability * thrustAttackProbabilityMultiplier)
            {
                inputManager.tryingToWeaponThrustAttack = true;
                inputManager.SendActionToInputBuffer(BufferActions.ACTION_WEAPON_THRUST_ATTACK);
            }
            else
            {
                // Force a random angle for each attack
                float randomAngle = Random.Range(Random.Range(0f, 360f), Random.Range(0f, 360f));

                if (forcedAngle != 0f)
                    randomAngle = forcedAngle;

                float oppositeAngle = randomAngle + 180 > 360 ? randomAngle - 180 : randomAngle + 180;

                weaponDial.manualAngle = randomAngle;

                float randomWeaponPart = Random.Range(0f, 100f);

                if (randomWeaponPart >= bottomAttackProbability)
                {
                    weaponDial.topAngle = randomAngle;
                    weaponDial.bottomAngle = oppositeAngle;
                    inputManager.tryingToWeaponTopAttack = true;
                    inputManager.SendActionToInputBuffer(BufferActions.ACTION_WEAPON_TOP_ATTACK);
                }
                else
                {
                    weaponDial.bottomAngle = randomAngle;
                    weaponDial.topAngle = oppositeAngle;
                    inputManager.tryingToWeaponBottomAttack = true;
                    inputManager.SendActionToInputBuffer(BufferActions.ACTION_WEAPON_BOTTOM_ATTACK);
                }

            }

            moveDirection = Random.Range(0, 2) * 2 - 1;
        }
        else
        {
            actionAttackTriggerTime -= Time.deltaTime;
        }
    }

    private void Dodge()
    {
        if(targetCharacter.isWeaponColliderActive && !triedEvading && !hasPerformedDefensiveAction)
        {
            triedEvading = true;

            float randomEvade = Random.Range(0f, 100f);
            if(randomEvade <= evadeProbability * evadeProbabilityMultiplier)
            {
                hasPerformedDefensiveAction = true;

                float randomBackstep = Random.Range(0f, 100f);
                if (randomBackstep <= backstepProbability)
                {
                    inputManager.tryingToBackstep = true;
                    inputManager.SendActionToInputBuffer(BufferActions.ACTION_BACKSTEP);
                }
                else
                {
                    switch (targetCharacter.attackInfo.type)
                    {
                        case AttackType.SLASH_WEAPON_TOP:
                            if (targetCharacter.attackInfo.topAngle <= 180)
                            {
                                inputManager.tryingToDodgeLeft = true;
                                inputManager.SendActionToInputBuffer(BufferActions.ACTION_DODGE_LEFT);
                            }
                            else
                            {
                                inputManager.tryingToDodgeRight = true;
                                inputManager.SendActionToInputBuffer(BufferActions.ACTION_DODGE_RIGHT);
                            }
                            break;
                        case AttackType.SLASH_WEAPON_BOTTOM:
                            if (targetCharacter.attackInfo.bottomAngle <= 180)
                            {
                                inputManager.tryingToDodgeLeft = true;
                                inputManager.SendActionToInputBuffer(BufferActions.ACTION_DODGE_LEFT);
                            }
                            else
                            {
                                inputManager.tryingToDodgeRight = true;
                                inputManager.SendActionToInputBuffer(BufferActions.ACTION_DODGE_RIGHT);
                            }
                            break;
                        case AttackType.THRUST:
                            float randomDodgeDirection = Random.Range(0f, 1f);
                            if (randomDodgeDirection < 0.5f)
                            {
                                inputManager.tryingToDodgeRight = true;
                                inputManager.SendActionToInputBuffer(BufferActions.ACTION_DODGE_RIGHT);
                            }
                            else
                            {
                                inputManager.tryingToDodgeLeft = true;
                                inputManager.SendActionToInputBuffer(BufferActions.ACTION_DODGE_LEFT);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    private void Parry()
    {
        if (targetCharacter.isWeaponColliderActive && !triedParrying && !hasPerformedDefensiveAction)
        {
            triedParrying = true;

            float randomParry = Random.Range(0f, 100f);

            float randomCounterThrust = Random.Range(0f, 100f);

            switch (targetCharacter.attackInfo.type)
            {
                case AttackType.SLASH_WEAPON_TOP:
                    if (randomParry <= parryProbability * parryProbabilityMultiplier)
                    {
                        weaponDial.manualAngle = 360 - targetCharacter.attackInfo.topAngle;
                        weaponDial.isUIWeaponAttached = true;
                        hasPerformedDefensiveAction = true;
                    }
                    break;
                case AttackType.SLASH_WEAPON_BOTTOM:
                    if (randomParry <= parryProbability * parryProbabilityMultiplier)
                    {
                        weaponDial.manualAngle = 360 - targetCharacter.attackInfo.bottomAngle;
                        weaponDial.isUIWeaponAttached = true;
                        hasPerformedDefensiveAction = true;
                    }
                    break;
                case AttackType.THRUST:
                    if (randomCounterThrust <= counterThrustProbability)
                    {
                        inputManager.tryingToWeaponThrustAttack = true;
                        hasPerformedDefensiveAction = true;
                    }
                    break;
                default:
                    break;
            }
        }
        else if (targetCharacter.isWeaponColliderActive && triedParrying && hasPerformedDefensiveAction)
            weaponDial.isUIWeaponAttached = true;
    }

    private void Guard()
    {
        if (targetCharacter.isWeaponColliderActive && !triedGuarding && !hasPerformedDefensiveAction)
        {
            triedGuarding = true;

            float randomGuard = Random.Range(0f, 100f);
            float accuracy = Random.Range(guardAccuracy, 100f);
            float offset = 30f - 20f * accuracy / 100f;

            switch (targetCharacter.attackInfo.type)
            {
                case AttackType.SLASH_WEAPON_TOP:
                    if (randomGuard <= guardProbability * guardProbabilityMultiplier)
                    {
                        weaponDial.manualAngle = targetCharacter.attackInfo.topAngle + offset > 360f ? targetCharacter.attackInfo.topAngle + offset  - 360f: 360f - targetCharacter.attackInfo.topAngle + offset;
                        weaponDial.isUIWeaponAttached = true;
                        hasPerformedDefensiveAction = true;
                    }
                    break;
                case AttackType.SLASH_WEAPON_BOTTOM:
                    if (randomGuard <= guardProbability * guardProbabilityMultiplier)
                    {
                        weaponDial.manualAngle = targetCharacter.attackInfo.bottomAngle + offset > 360 ? targetCharacter.attackInfo.bottomAngle + offset - 360 : targetCharacter.attackInfo.bottomAngle + offset;
                        weaponDial.isUIWeaponAttached = true;
                        hasPerformedDefensiveAction = true;
                    }
                    break;
                
                default:
                    break;
            }
        }
        else if(targetCharacter.isWeaponColliderActive && triedGuarding && hasPerformedDefensiveAction)
            weaponDial.isUIWeaponAttached = true;
    }

   private void SetProbabilities(float multiplier)
   {
        evadeProbabilityMultiplier = 1 + multiplier;

        parryProbabilityMultiplier = 1f + (multiplier * multiplier / 4f);

        guardProbabilityMultiplier = 1f + multiplier;

        if (targetCharacter.isAttacking && targetCharacter.attackInfo.type == AttackType.THRUST)
                evadeProbabilityMultiplier += 1;

        if (character.isBackstepping)
        {
            actionAttackTriggerTime = (actionAttackTriggerTime + 0.6f) / 3f;
            thrustAttackProbabilityMultiplier = 15f;
        }
        else if (!character.isMovementRestriced)
            thrustAttackProbabilityMultiplier = 1f;
    }

}
