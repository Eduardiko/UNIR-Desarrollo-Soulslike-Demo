using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SpriteType
{
    ATTACK,
    GUARD,
    CANCELLED,
    NONE
}

public class WeaponDial : MonoBehaviour
{
    [Header("Weapon Transforms")]
    [SerializeField] private Transform topRWeaponTransform;
    [SerializeField] private Transform bottomRWeaponTransform;
    [SerializeField] private Transform topLWeaponTransform;
    [SerializeField] private Transform bottomLWeaponTransform;

    [Header("Weapon UI Sprites")]
    [SerializeField] private GameObject topWeaponUI;
    [SerializeField] private GameObject bottomWeaponUI;
    [SerializeField] private GameObject targetTopWeaponUI;
    [SerializeField] private GameObject targetBottomWeaponUI;

    [SerializeField] private Sprite attackTopWeaponSprite;
    [SerializeField] private Sprite attackBottomWeaponSprite;
    [SerializeField] private Sprite guardTopWeaponSprite;
    [SerializeField] private Sprite guardBottomWeaponSprite;
    [SerializeField] private Sprite cancelledTopWeaponSprite;
    [SerializeField] private Sprite cancelledBottomWeaponSprite;

    [SerializeField] private Sprite targetTopWeaponSprite;
    [SerializeField] private Sprite targetBottomWeaponSprite;
    [SerializeField] private Sprite targetUnabledTopWeaponSprite;
    [SerializeField] private Sprite targetUnabledBottomWeaponSprite;

    [SerializeField] private RectTransform manualPointerRect;
    

    [Header("Plane Reference")]
    [SerializeField] private Transform referencePlaneTransform;

    // References
    private Character character;
    private InputManager inputManager;

    // Variables
    [HideInInspector] public float topAngle;
    [HideInInspector] public float bottomAngle;
    [HideInInspector] public float manualAngle;
    [HideInInspector] public bool isUIWeaponAttached = true;

    private float radius;

    private Vector3 planeNormal;

    private Vector3 topProjection;
    private Vector3 bottomProjection;

    private Vector3 topRefPoint;
    private Vector3 centerRefPoint;
    private Vector3 bottomRefPoint;

    private Transform topWeaponTransform;
    private Transform bottomWeaponTransform;

    // UI
    private Image topWeaponImage;
    private Image bottomWeaponImage;
    private Image targetTopWeaponImage;
    private Image targetBottomWeaponImage;

    private RectTransform topWeaponRect;
    private RectTransform bottomWeaponRect;
    private RectTransform targetTopWeaponRect;
    private RectTransform targetBottomWeaponRect;

    private SpriteType spriteType;

    private void Start()
    {
        character = GetComponent<Character>();
        inputManager = GetComponent<InputManager>();

        topWeaponRect = topWeaponUI.GetComponent<RectTransform>();
        topWeaponImage = topWeaponUI.GetComponent<Image>();

        bottomWeaponRect = bottomWeaponUI.GetComponent<RectTransform>();
        bottomWeaponImage = bottomWeaponUI.GetComponent<Image>();

        targetTopWeaponRect = targetTopWeaponUI.GetComponent<RectTransform>();
        targetTopWeaponImage = targetTopWeaponUI.GetComponent<Image>();

        targetBottomWeaponRect = targetBottomWeaponUI.GetComponent<RectTransform>();
        targetBottomWeaponImage = targetBottomWeaponUI.GetComponent<Image>();

        SetAttackSprites();
    }

    private void Update()
    {
        if(!character.isDead)
        {
            UpdateWeapon();

            SetAngles();
            UpdateUI();
        }
    }

    #region ANGLES

    private void SetAngles()
    {
        if (!isUIWeaponAttached)
            SetTopBottomAngles();
        else
            SetAttachedTopBottomAngles();

        ManageManualAngle();
    }

    private void SetTopBottomAngles()
    {
        planeNormal = referencePlaneTransform.up;

        // Calculate the projection of the two points onto the plane
        topProjection = Vector3.ProjectOnPlane(topWeaponTransform.position - referencePlaneTransform.position, planeNormal) + referencePlaneTransform.position;
        bottomProjection = Vector3.ProjectOnPlane(bottomWeaponTransform.position - referencePlaneTransform.position, planeNormal) + referencePlaneTransform.position;

        // Calculate the center and radius of the circle that passes through both projection points
        centerRefPoint = (topProjection + bottomProjection) / 2.0f;
        radius = Vector3.Distance(centerRefPoint, topProjection);

        // Calculate the points to use as reference
        topRefPoint = centerRefPoint + Vector3.up * radius;
        bottomRefPoint = centerRefPoint + Vector3.down * radius;

        // Calculate the angles
        Vector3 centerToRef;
        Vector3 centerToPoint;

        // Convert the {-180, 180} returned by the SignedAngle function to {0, 360} for QoL -> condition ? true : false
        centerToRef = topRefPoint - centerRefPoint;
        centerToPoint = topProjection - centerRefPoint;
        topAngle = Vector3.SignedAngle(centerToPoint, centerToRef, planeNormal);
        topAngle = topAngle < 0 ? 360f + topAngle : topAngle;

        centerToRef = bottomRefPoint - centerRefPoint;
        centerToPoint = topProjection - centerRefPoint;
        bottomAngle = Vector3.SignedAngle(centerToPoint, centerToRef, planeNormal);
        bottomAngle = bottomAngle < 0 ? 360f + bottomAngle : bottomAngle;
    }

    private void SetAttachedTopBottomAngles()
    {
        // Attach Top Angle and set Bottom Angle to + 180º
        topAngle = manualAngle;
        bottomAngle = manualAngle + 180 > 360 ? manualAngle + 180 - 360 : manualAngle + 180;
    }

    private void ManageManualAngle()
    {
        if (inputManager.inputWeaponDialVector != Vector2.zero && character.isLocking)
        {
            // Set UI Active to be rendered
            manualPointerRect.gameObject.SetActive(true);
            isUIWeaponAttached = true;

            // Calculate manual angle
            float angleRadians = Mathf.Atan2(inputManager.inputWeaponDialVector.x, inputManager.inputWeaponDialVector.y);
            manualAngle = angleRadians * Mathf.Rad2Deg;
            manualAngle = manualAngle < 0 ? 360f + manualAngle : manualAngle;
        }
        else
        {
            // Unnattach weapon and stop rendering UI
            isUIWeaponAttached = false;
            manualPointerRect.gameObject.SetActive(false);
        }
    }

    // ---- Uncomment To Use Threshold System - Add at the beggining ----

    //[Header("Parameters")]
    //[SerializeField] private float attachAcceptanceThreshold = 15f;

    // ---- Uncomment To Use Threshold System - Replace SetAttachedTopBottomAngles() code ----

    //float angularTopDifference = Mathf.Abs(Mathf.DeltaAngle(manualAngle, topAngle));
    //float angularBottomDifference = Mathf.Abs(Mathf.DeltaAngle(manualAngle, bottomAngle));

    //// Attach the nearest part to the manual angle
    //if (angularTopDifference < angularBottomDifference)
    //{
    //    topAngle = manualAngle;
    //    bottomAngle = manualAngle + 180 > 360 ? manualAngle + 180 - 360 : manualAngle + 180;
    //}
    //else
    //{
    //    bottomAngle = manualAngle;
    //    topAngle = manualAngle + 180 > 360 ? manualAngle + 180 - 360 : manualAngle + 180;
    //}

    // ---- Uncomment To Use Threshold System - Add code in ManageManualAngle() ----

    //float angularTopDifference = Mathf.Abs(Mathf.DeltaAngle(manualAngle, topAngle));
    //float angularBottomDifference = Mathf.Abs(Mathf.DeltaAngle(manualAngle, bottomAngle));
    //if (angularTopDifference <= attachAcceptanceThreshold || angularBottomDifference <= attachAcceptanceThreshold)
    //    isUIWeaponAttached = true;

    #endregion

    #region UI
    private void UpdateUI()
    {
        if(!character.isStaggered)
            SetAttackSprites();

        if (isUIWeaponAttached || !character.isUILocked)
            UpdateAnglesUI();

        if (character.isLocking && character.target != null)
            UpdateTargetAnglesUI();
        else
            DisableTargetUI();
    }

    private void UpdateAnglesUI()
    {
        // Top Angle
        // Convert angle to radians
        float radianTopAngle = (90 - topAngle) * Mathf.Deg2Rad;

        // Calculate the new position
        float x = 0.5f * Mathf.Cos(radianTopAngle);
        float y = 0.5f * Mathf.Sin(radianTopAngle);

        // Apply the new position
        topWeaponRect.localPosition = new Vector3(x, y, topWeaponRect.localPosition.z);

        if(spriteType == SpriteType.ATTACK)
        {
            // Rotate the topWeaponRect to look at the circle center
            Vector3 directionToTarget = Vector3.zero - topWeaponRect.localPosition;
            float targetRotation = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            topWeaponRect.localRotation = Quaternion.Euler(0f, 0f, targetRotation + 90f);
        }
        else
            topWeaponRect.localRotation = Quaternion.Euler(0f, 0f, 0f);


        // Bottom Angle
        float radianBottomAngle = (90 - bottomAngle) * Mathf.Deg2Rad;
        x = 0.5f * Mathf.Cos(radianBottomAngle);
        y = 0.5f * Mathf.Sin(radianBottomAngle);
        bottomWeaponRect.localPosition = new Vector3(x, y, bottomWeaponRect.localPosition.z);

        if (spriteType == SpriteType.ATTACK)
        {
            Vector3 directionToTarget = Vector3.zero - bottomWeaponRect.localPosition;
            float targetRotation = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            bottomWeaponRect.localRotation = Quaternion.Euler(0f, 0f, targetRotation - 90f);
        }
        else
            bottomWeaponRect.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // Manual Angle
        if (manualPointerRect.gameObject.activeSelf)
        {
            float radianManualAngle = (90 - manualAngle) * Mathf.Deg2Rad;
            x = 0.5f * Mathf.Cos(radianManualAngle);
            y = 0.5f * Mathf.Sin(radianManualAngle);
            manualPointerRect.localPosition = new Vector3(x, y, manualPointerRect.localPosition.z);

            if (spriteType == SpriteType.ATTACK)
            {
                Vector3 directionToTarget = Vector3.zero - manualPointerRect.localPosition;
                float targetRotation = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
                manualPointerRect.localRotation = Quaternion.Euler(0f, 0f, targetRotation + 90f);
            }
            else
                manualPointerRect.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    private void UpdateTargetAnglesUI()
    {
        Character targetCharacter = character.target.GetComponent<Character>();

        // Activate Render
        if (targetCharacter.isUILocked && targetCharacter.attackInfo.type != AttackType.NONE)
        {
            targetTopWeaponRect.gameObject.SetActive(true);
            targetBottomWeaponRect.gameObject.SetActive(true);
        }
        else
        {
            targetTopWeaponRect.gameObject.SetActive(false);
            targetBottomWeaponRect.gameObject.SetActive(false);
        }

        // Target Angles (Enemy Angles)
        if ((targetTopWeaponRect.gameObject.activeSelf || targetBottomWeaponRect.gameObject.activeSelf) && targetCharacter.attackInfo.type != AttackType.NONE)
        {
            if (targetCharacter.attackInfo.type != AttackType.SLASH_WEAPON_BOTTOM)
                SetTopAttackTargetSprites();
            else
                SetBottomAttackTargetSprites();

            float radianTopAngle = (90 - targetCharacter.attackInfo.topAngle) * Mathf.Deg2Rad;
            float x = 0.5f * Mathf.Cos(radianTopAngle);
            float y = 0.5f * Mathf.Sin(radianTopAngle);
            targetTopWeaponRect.localPosition = new Vector3(-x, y, targetTopWeaponRect.localPosition.z);
            
            Vector3 directionToTarget = Vector3.zero - targetTopWeaponRect.localPosition;
            float targetRotation = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            targetTopWeaponRect.localRotation = Quaternion.Euler(0f, 0f, targetRotation + 90f);

            float radianBottomAngle = (90 - targetCharacter.attackInfo.bottomAngle) * Mathf.Deg2Rad;
            x = 0.5f * Mathf.Cos(radianBottomAngle);
            y = 0.5f * Mathf.Sin(radianBottomAngle);
            targetBottomWeaponRect.localPosition = new Vector3(-x, y, targetBottomWeaponRect.localPosition.z);

            directionToTarget = Vector3.zero - targetBottomWeaponRect.localPosition;
            targetRotation = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            targetBottomWeaponRect.localRotation = Quaternion.Euler(0f, 0f, targetRotation - 90f);
        }
    }

    private void DisableTargetUI()
    {
        targetTopWeaponRect.gameObject.SetActive(false);
        targetBottomWeaponRect.gameObject.SetActive(false);
    }

    #endregion

    #region SPRITES

    private void SetAttackSprites()
    {
        spriteType = SpriteType.ATTACK;
        topWeaponImage.sprite = attackTopWeaponSprite;
        bottomWeaponImage.sprite = attackBottomWeaponSprite;
        bottomWeaponRect.localScale = new Vector3(0.4f, 0.4f, 0.4f);
    }

    public void SetGuardSprites()
    {
        spriteType = SpriteType.GUARD;
        topWeaponImage.sprite = guardTopWeaponSprite;
        bottomWeaponImage.sprite = guardBottomWeaponSprite;
        bottomWeaponRect.localScale *= 1f;
    }

    public void SetCancelledSprites()
    {
        spriteType = SpriteType.CANCELLED;
        topWeaponImage.sprite = cancelledTopWeaponSprite;
        bottomWeaponImage.sprite = cancelledBottomWeaponSprite;
        bottomWeaponRect.localScale *= 0.5f;
    }

    private void SetTopAttackTargetSprites()
    {
        targetTopWeaponImage.sprite = targetTopWeaponSprite;
        targetBottomWeaponImage.sprite = targetUnabledBottomWeaponSprite;
    }

    private void SetBottomAttackTargetSprites()
    {
        targetBottomWeaponImage.sprite = targetBottomWeaponSprite;
        targetTopWeaponImage.sprite = targetUnabledTopWeaponSprite;
    }

    #endregion

    #region HELPERS

    private void UpdateWeapon()
    {
        // Update Top/Bottom Transforms to match active weapon (Left/Right)
        if(character.RWeapon.activeSelf)
        {
            topWeaponTransform = topRWeaponTransform;
            bottomWeaponTransform = bottomRWeaponTransform;
        } else
        {
            topWeaponTransform = topLWeaponTransform;
            bottomWeaponTransform = bottomLWeaponTransform;
        }
    }


    void OnDrawGizmos()
    {
        // Draw the projection points and circle
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(topProjection, 0.1f);
        Gizmos.DrawSphere(bottomProjection, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(centerRefPoint, 0.1f);
        Gizmos.DrawWireSphere(centerRefPoint, radius);

        // Draw Lines For Top Point
        Gizmos.color = Color.green;
        Gizmos.DrawLine(centerRefPoint, topRefPoint);
        Gizmos.DrawLine(topProjection, centerRefPoint);

        // Draw Lines For Bottom Point
        Gizmos.color = Color.green;
        Gizmos.DrawLine(centerRefPoint, bottomRefPoint);
        Gizmos.DrawLine(bottomProjection, centerRefPoint);
    }


    private void LockUI()
    {
        character.isUILocked = true;
    }

    private void UnlockUI()
    {
        character.isUILocked = false;
    }


    #endregion
}
