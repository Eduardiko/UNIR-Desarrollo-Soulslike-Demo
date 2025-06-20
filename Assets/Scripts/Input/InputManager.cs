using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public enum BufferActions
{
    ACTION_RUN,
    ACTION_JUMP,
    ACTION_WEAPON_TOP_ATTACK,
    ACTION_WEAPON_BOTTOM_ATTACK,
    ACTION_WEAPON_THRUST_ATTACK,
    ACTION_BACKSTEP,
    ACTION_DODGE_RIGHT,
    ACTION_DODGE_LEFT,
    CLEAR
}

public class InputManager : MonoBehaviour
{
    // Movement Inputs
    [HideInInspector] public bool tryingToRun = false;                  
    [HideInInspector] public bool tryingToJump = false;                
    [HideInInspector] public bool tryingToMove = false;                 
    [HideInInspector] public bool tryingToBackstep = false;
    [HideInInspector] public bool tryingToDodgeRight = false;
    [HideInInspector] public bool tryingToDodgeLeft = false;
    [HideInInspector] public Vector2 inputMoveVector = new Vector2();

    // Camera Inputs
    [HideInInspector] public bool tryingToLock = false;
    [HideInInspector] public bool tryingToLook = false;
    [HideInInspector] public Vector2 inputLookVector = new Vector2();

    // Offensive Inputs
    [HideInInspector] public bool tryingToWeaponTopAttack = false;
    [HideInInspector] public bool tryingToWeaponBottomAttack = false;
    [HideInInspector] public bool tryingToWeaponThrustAttack = false;
    [HideInInspector] public Vector2 inputWeaponDialVector = new Vector2();

    // Input Manager Variables
    [HideInInspector] public BufferActions bufferedAction;
    private float bufferTime = 0.20f;
    private float bufferedTimeRemaining = 0f;
    private bool isBuffering = false;

    void Update()
    {
        UpdateInputBuffer();
        UpdateConstantBools();
    }

    #region INPUT BUFFER

    public void SendActionToInputBuffer(BufferActions actionToBuffer)
    {
        isBuffering = true;
        bufferedAction = actionToBuffer;
        bufferedTimeRemaining = bufferTime;
    }

    private void UpdateInputBuffer()
    {
        if (isBuffering)
        {
            if (bufferedTimeRemaining >= 0)
            {
                switch (bufferedAction)
                {
                    case BufferActions.ACTION_RUN:
                        tryingToRun = true;
                        break;
                    case BufferActions.ACTION_JUMP:
                        tryingToJump = true;
                        break;
                    case BufferActions.ACTION_WEAPON_TOP_ATTACK:
                        tryingToWeaponTopAttack = true;
                        break;
                    case BufferActions.ACTION_WEAPON_BOTTOM_ATTACK:
                        tryingToWeaponBottomAttack = true;
                        break;
                    case BufferActions.ACTION_WEAPON_THRUST_ATTACK:
                        tryingToWeaponThrustAttack = true;
                        break;
                    case BufferActions.ACTION_BACKSTEP:
                        tryingToBackstep = true;
                        break;
                    case BufferActions.ACTION_DODGE_RIGHT:
                        tryingToDodgeRight = true;
                        break;
                    case BufferActions.ACTION_DODGE_LEFT:
                        tryingToDodgeLeft = true;
                        break;
                    default:
                        break;
                }
            } else
            {
                isBuffering = false;
            }

            bufferedTimeRemaining -= Time.deltaTime;
        }
    }

    #endregion

    #region ACTIONS

    public void UpdateConstantBools()
    {
        if (inputMoveVector != Vector2.zero)
            tryingToMove = true;
        else
            tryingToMove = false;

        if (inputLookVector != Vector2.zero)
            tryingToLook = true;
        else
            tryingToLook = false;
    }

    // Movement Actions
    public void ActionJump(InputAction.CallbackContext context)
    {
        // For this prototype the jump is left out. Uncomment to reactivate jump

        //if (context.performed)
        //{
        //    tryingToJump = true;
        //    SendActionToInputBuffer(BufferActions.ACTION_JUMP);
        //}
    }

    public void ActionRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            tryingToRun = true;
            SendActionToInputBuffer(BufferActions.ACTION_RUN);
        }
    }

    public void ActionBackstep(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            tryingToBackstep = true;
            SendActionToInputBuffer(BufferActions.ACTION_BACKSTEP);
        }
    }

    public void ActionDodgeRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            tryingToDodgeRight = true;
            SendActionToInputBuffer(BufferActions.ACTION_DODGE_RIGHT);
        }
    }

    public void ActionDodgeLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            tryingToDodgeLeft = true;
            SendActionToInputBuffer(BufferActions.ACTION_DODGE_LEFT);
        }
    }

    public void ActionMove(InputAction.CallbackContext context)
    {
        inputMoveVector = context.ReadValue<Vector2>();
    }

    // Camera Actions
    public void ActionLook(InputAction.CallbackContext context)
    {
        inputLookVector = context.ReadValue<Vector2>();
    }

    public void ActionLock(InputAction.CallbackContext context)
    {
        if (context.performed) tryingToLock = true;
    }

    // Offensive Actions
    public void ActionWeaponTopAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            tryingToWeaponTopAttack = true;
            SendActionToInputBuffer(BufferActions.ACTION_WEAPON_TOP_ATTACK);
        }
    }

    public void ActionWeaponBottomAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            tryingToWeaponBottomAttack = true;
            SendActionToInputBuffer(BufferActions.ACTION_WEAPON_BOTTOM_ATTACK);
        }
    }

    public void ActionWeaponThrustAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            tryingToWeaponThrustAttack = true;
            SendActionToInputBuffer(BufferActions.ACTION_WEAPON_THRUST_ATTACK);
        }
    }

    public void ActionAdjustWeaponDial(InputAction.CallbackContext context)
    {
        inputWeaponDialVector = context.ReadValue<Vector2>();
    }

    #endregion
}
