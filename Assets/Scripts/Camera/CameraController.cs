using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lockDetectionRadius;
    [SerializeField] private float lookAtSmoothing;
    [SerializeField] private float maxLockAngle;

    [Header("References")]
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineVirtualCamera lockCamera;
    [SerializeField] private GameObject uiLock;
    [SerializeField] private LayerMask enemyLayerMask;

    // References
    private Character character;
    private InputManager inputManager;

    // Variables
    private List<GameObject> lockableEnemies = new List<GameObject>();
    private Vector3 playerToNearestEnemyVector = new Vector3();

    private void Start()
    {
        character = GetComponent<Character>();
        inputManager = GetComponent<InputManager>();

        uiLock.SetActive(false);
    }

    private void FixedUpdate()
    {
        ManageRecentering();
        ManageLocking();
    }

    #region MANAGERS
    private void ManageRecentering()
    {
        //Locking && Looking - have priority over walking
        if (character.isLocking || inputManager.tryingToLook)
        {
            freeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
            freeLookCamera.m_YAxisRecentering.m_enabled = false;
            return;
        }

        // Walking
        if (!inputManager.tryingToMove || inputManager.inputMoveVector.y < -0.95)
        {
            freeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
            freeLookCamera.m_YAxisRecentering.m_enabled = false;
        }
        else if (inputManager.inputMoveVector.y >= -0.7)
        {
            freeLookCamera.m_RecenterToTargetHeading.m_enabled = true;
            freeLookCamera.m_YAxisRecentering.m_enabled = true;
        }
        else if (inputManager.inputMoveVector.y < -0.7)
        {
            freeLookCamera.m_RecenterToTargetHeading.m_enabled = true;
            freeLookCamera.m_YAxisRecentering.m_enabled = false;
        }
        else
        {
            freeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
            freeLookCamera.m_YAxisRecentering.m_enabled = false;
        }
    }

    private void ManageLocking()
    {
        if (inputManager.tryingToLock && !character.isLocking)
        {
            // If there are not available enemies, don't change the camera
            inputManager.tryingToLock = false;
            if (FindLockableTargets())
                SetLockCamera();
        }
        else if (inputManager.tryingToLock && character.isLocking)
        {
            // If locking, return to freeLook
            inputManager.tryingToLock = false;
            SetFreeLookCamera();
        }

        // Updates camera so player is always in-sight
        if (character.isLocking)
            UpdateLockedCamera();
        else
            SetFreeLookCamera();
    }
    #endregion

    #region SETTERS
    private void SetFreeLookCamera()
    {
        // Set Bools
        uiLock.SetActive(false);
        character.isLocking = false;

        // Change Cameras
        freeLookCamera.gameObject.SetActive(true);
        lockCamera.gameObject.SetActive(false);
    }

    private void SetLockCamera()
    {
        // Set Booleans
        uiLock.SetActive(true);
        character.isLocking = true;

        // Change Cameras
        lockCamera.gameObject.SetActive(true);
        freeLookCamera.gameObject.SetActive(false);
    }

    #endregion

    #region HELPERS
    private bool FindLockableTargets()
    {
        lockableEnemies.Clear();

        // Find all nearby GameObjects && LookAt Vector of the camera
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, lockDetectionRadius, enemyLayerMask);
        Vector3 lookDir = freeLookCamera.m_LookAt.transform.position - freeLookCamera.transform.position;

        float closestAngle = maxLockAngle;

        foreach (Collider collider in hitColliders)
        {
            // Calculate angle between the relative direction w/enemy and the "center of screen" vectors to determine if they are inside the range
            Vector3 dir = collider.gameObject.transform.position - freeLookCamera.transform.position;
            float angle = Vector3.Angle(lookDir, dir);

            if (collider.gameObject.tag == "Enemy" && angle <= maxLockAngle)
            {
                lockableEnemies.Add(collider.gameObject);

                // Set Lockable Target
                if (angle < closestAngle)
                {
                    closestAngle = angle;
                    character.target = collider.gameObject;
                }
            }
        }

        if (lockableEnemies.Count == 0) return false;

        return true;
    }

    private void UpdateLockedCamera()
    {
        Transform targetLookAtTransform = character.target.GetComponent<Character>().lookAtTransform;
        lockCamera.LookAt = targetLookAtTransform;

        // Change To FreeLook Camera if being far from the locked enemy
        playerToNearestEnemyVector = character.target.transform.position - transform.position;
        if (playerToNearestEnemyVector.magnitude > lockDetectionRadius + lockDetectionRadius / 6f)
            SetFreeLookCamera();

        // Rotates the camera so that the forward Vector is always the Vector between enemy & player
        Vector3 direction = targetLookAtTransform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        lockCamera.transform.rotation = Quaternion.Slerp(lockCamera.transform.rotation, rotation, lookAtSmoothing * Time.deltaTime);

        // Updates the position & scale of the UI
        Vector3 targetPos = targetLookAtTransform.position;
        uiLock.transform.position = targetPos;
        uiLock.transform.localScale = Vector3.one * ((lockCamera.transform.position - targetPos).magnitude);
    }

    #endregion

}
