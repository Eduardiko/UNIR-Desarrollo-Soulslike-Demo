using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Strings To Re-Use and Not memorize all animation parameters
[System.Serializable]
public class AnimationKeys
{
    // Triggers
    public string jumpTriggerKey;
    public string runTriggerKey;
    public string attackTriggerKey;
    public string dodgeTriggerKey;
    public string hitTriggerKey;

    // Bools
    public string isRunningKey;
    public string isLockingKey;

    // Floats
    public string directionXKey;
    public string directionZKey;
    public string attackDirection;
    public string dodgeDirection;
    public string hitID;

    // Ints
    public string comboKey;
}

public enum AttackType
{
    SLASH_WEAPON_TOP,
    SLASH_WEAPON_BOTTOM,
    THRUST,
    NONE
}

public struct AttackInfo
{
    public float damageAmmount;
    public float topAngle;
    public float bottomAngle;
    public AttackType type;
}

public class Character : MonoBehaviour
{
    // Character Parameters
    [Header("Parameters")]
    public float maxHealth = 100f;
    public float health = 100f;
    public Transform lookAtTransform;
    [SerializeField] private float attackStepLengthMultiplier = 1;
    [SerializeField] private float attackStepTime;
    [SerializeField] private float stepLengthMultiplier = 1;
    [SerializeField] private float stepTime;

    // Access Character Animations
    [Header("Animations")]
    [SerializeField] public AnimationKeys animKeys;
    [HideInInspector] public Animator animator;
    [HideInInspector] public AudioSource audioSource;

    // Fake It Until U Make It References
    public GameObject RWeapon;
    public GameObject LWeapon;

    // State Bools - Changed By Code
    [HideInInspector] public bool isGrounded = true;
    [HideInInspector] public bool isRunning = false;
    [HideInInspector] public bool isLocking = false;
    [HideInInspector] public bool isDead = false;

    // State Bools - Changed By Animations - To Have More Control
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public bool isWeaponColliderActive = false;

    [HideInInspector] public bool isDodging = false;
    [HideInInspector] public bool isBackstepping = false;
    [HideInInspector] public bool isStaggered = false;
    [HideInInspector] public bool isImmuneToDamage = false;

    [HideInInspector] public bool isUILocked = false;
    [HideInInspector] public bool isMovementRestriced = false;

    // State Bools - General Ones - Dependent previous states
    [HideInInspector] public bool isPerformingAnAction = false;

    // Combat Information
    [HideInInspector] public AttackInfo attackInfo;
    [HideInInspector] public GameObject target;
    [HideInInspector] public int combo = 0;

    // Fixed Update
    private float elapsedTime = 0;
    [HideInInspector] public bool doStep = false;
    [HideInInspector] public bool doBackStep = false;
    private int stepDirection = 0;
    [HideInInspector] public Coroutine currentCoroutine;

    private void Start()
    {
        // Although it is set "true" in awake, deleting it fom here makes enemies not work
        isGrounded = true;

        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponentInChildren<AudioSource>();
    }

    // Fixed Update is used so that step does not trigger
    private void FixedUpdate()
    {
        if (doStep)
            FixedStep();

        if (doBackStep)
            FixedBackStep();
    }


    #region MAIN

    private void UpdateGeneralBools()
    {
        // isPerformingAnAction is the main bool that determines when an action has become uncancellable
        if (isAttacking || isBackstepping || isDodging || isStaggered || isDead)
            isPerformingAnAction = true;
        else
            isPerformingAnAction = false;
    }

    #endregion

    #region STATES

    // Set any other state to false if the action can interrupt mid animation - Ej. IsStaggered()
    // Offense States
    private void IsAttacking()
    {
        isAttacking = true;
        UpdateGeneralBools();
    }

    private void IsNotAttacking()
    {
        isAttacking = false;
        UpdateGeneralBools();
    }

    // Defense States
    private void IsDodging()
    {
        isDodging = true;
        UpdateGeneralBools();
    }

    private void IsNotDodging()
    {
        isDodging = false;
        UpdateGeneralBools();
    }

    private void IsBackstepping()
    {
        isBackstepping = true;
        UpdateGeneralBools();
    }

    private void IsNotBackstepping()
    {
        isBackstepping = false;
        UpdateGeneralBools();
    }

    private void IsImmuneToDamage()
    {
        isImmuneToDamage = true;
        UpdateGeneralBools();
    }

    private void IsNotImmuneToDamage()
    {
        isImmuneToDamage = false;
        UpdateGeneralBools();
    }

    // General States
    private void IsMovementRestricted()
    {
        isMovementRestriced = true;
        UpdateGeneralBools();
    }

    private void IsNotMovementRestricted()
    {
        isMovementRestriced = false;
        UpdateGeneralBools();
    }

    private void IsStaggered()
    {
        isStaggered = true;

        isAttacking = false;
        isDodging = false;
        isBackstepping = false;
        isUILocked = false;

        UpdateGeneralBools();
    }

    private void IsNotStaggered()
    {
        ResetStates();
        UpdateGeneralBools();
    }

    private void IsDead()
    {
        isDead = true;
        UpdateGeneralBools();
    }

    #endregion

    #region HELPERS

    public void SetAttackInfo(float damageAmmount, float topAngle, float bottomAngle, AttackType type)
    {
        attackInfo.damageAmmount = damageAmmount;
        attackInfo.topAngle = topAngle;
        attackInfo.bottomAngle = bottomAngle;
        attackInfo.type = type;
    }

    public void ClearAttackInfo()
    {
        attackInfo.damageAmmount = 0f;
        attackInfo.topAngle = 0f;
        attackInfo.bottomAngle = 180f;
        attackInfo.type = AttackType.NONE;
    }

    public void ResetStates()
    {
        isGrounded = true;
        isRunning = false;

        isAttacking = false;
        isWeaponColliderActive = false;
        
        isDodging = false;
        isBackstepping = false;
        isStaggered = false;
        isImmuneToDamage = false;

        isUILocked = false;
        isMovementRestriced = false;

        isPerformingAnAction = false;
    }

    private void FixedStep()
    {
        float stepDistance = Mathf.Lerp(attackStepLengthMultiplier, 0f, elapsedTime / stepTime) * Time.deltaTime;

        // Calculate the new position after stepping forward
        Vector3 newPosition = transform.position + transform.forward * stepDistance;

        // Move the GameObject to the new position
        transform.position = newPosition;
    }

    public IEnumerator Step(bool toTarget = false)
    {
        elapsedTime = 0;

        // Rotate the character to the target
        if ((isLocking || toTarget) && target != null)
        {
            // Y axis to 0 so Vector is calculated at same height
            Vector3 targetPos = new Vector3(target.transform.position.x, 0f, target.transform.position.z);
            Vector3 selfPos = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.rotation = Quaternion.LookRotation(targetPos - selfPos);
        }

        while (elapsedTime < attackStepTime)
        {
            doBackStep = false;

            if ((!toTarget && !isLocking) || (target != null && (target.transform.position - transform.position).magnitude > 1.2f))
                doStep = true;
            else
                doStep = false;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void FixedBackStep()
    {
        // Rotate the character to the target
        if (isLocking && target != null)
        {
            // Y axis to 0 so Vector is calculated at same height
            Vector3 targetPos = new Vector3(target.transform.position.x, 0f, target.transform.position.z);
            Vector3 selfPos = new Vector3(transform.position.x, 0f, transform.position.z);
            transform.rotation = Quaternion.LookRotation(targetPos - selfPos);
        }

        float stepDistance = Mathf.Lerp(stepLengthMultiplier, 0f, elapsedTime / stepTime);

        if (!isLocking)
            stepDistance *= 1.3f;

        Vector3 newPosition = new Vector3();

        switch (stepDirection)
        {
            case 0:
                // Calculate the new position for backstep
                newPosition = transform.position - transform.forward * stepDistance * 4f * Time.deltaTime;
                break;
            case 1:
                // Calculate the new position for right dodge
                newPosition = transform.position + transform.right * stepDistance * 2f * Time.deltaTime;
                break;
            case -1:
                // Calculate the new position for left dodge
                newPosition = transform.position - transform.right * stepDistance * 2f * Time.deltaTime;
                break;
            default:
                break;
        }

        // Move the GameObject to the new position
        transform.position = newPosition;
    }

    public IEnumerator BackStep(int direction=0)
    {
        elapsedTime = 0;
        stepDirection = direction;

        while (elapsedTime < stepTime)
        {
            doStep = false;

            elapsedTime += Time.deltaTime;

            doBackStep = true;
            if (elapsedTime >= stepTime)
                doBackStep = false;

            yield return null;
        }
    }

    public IEnumerator WaitToBackstep(float waitTime, int direction = 0)
    {
        elapsedTime = 0;
        
        if(currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        while (elapsedTime < waitTime)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= waitTime)
                StartCoroutine(BackStep(direction));

            yield return null;
        }
    }

    public void UpdateWeapon(bool lActive, bool rActive)
    {
        RWeapon.SetActive(rActive);
        LWeapon.SetActive(lActive);
    }

    #endregion
}
