using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Движение
    public Vector2 moveInput;
    public bool isRunning;

    // Прыжок и позы
    public bool jumpPressed;
    public bool crouchPressed;
    public bool altCrouchHeld;
    public bool pronePressed;

    // Оружие
    public bool fireHeld;
    public bool firePressed;
    public bool aimHeld;
    public bool reloadPressed;
    public bool inspectOrChangeHold;
    public bool cycleFireModePressed;
    public bool weaponSlot1Pressed;
    public bool weaponSlot2Pressed;

    // Гранаты
    public bool throwGrenadePressed;
    public bool cycleGrenadePressed;

    // Команда и прочее
    public bool openRadialMenuPressed;
    public bool interactPressed;
    public bool pauseMenuPressed;

    void Update()
    {
        // Движение
        float h = 0f, v = 0f;
        if (PlayerControls.MoveRight) h += 1f;
        if (PlayerControls.MoveLeft) h -= 1f;
        if (PlayerControls.MoveForward) v += 1f;
        if (PlayerControls.MoveBack) v -= 1f;
        moveInput = new Vector2(h, v).normalized;
        isRunning = PlayerControls.Run;

        // Позы и прыжок
        jumpPressed = PlayerControls.Jump;
        crouchPressed = PlayerControls.Crouch;
        altCrouchHeld = PlayerControls.AltCrouch;
        pronePressed = PlayerControls.Prone;

        // Оружие
        fireHeld = PlayerControls.FireHeld;
        firePressed = PlayerControls.FirePressed;
        aimHeld = PlayerControls.Aim;
        reloadPressed = PlayerControls.Reload;
        inspectOrChangeHold = PlayerControls.InspectOrChangeHold;
        cycleFireModePressed = PlayerControls.CycleFireMode;
        weaponSlot1Pressed = PlayerControls.WeaponSlot1;
        weaponSlot2Pressed = PlayerControls.WeaponSlot2;

        // Гранаты
        throwGrenadePressed = PlayerControls.ThrowGrenade;
        cycleGrenadePressed = PlayerControls.CycleGrenade;

        // Команда и UI
        openRadialMenuPressed = PlayerControls.OpenRadialMenu;
        interactPressed = PlayerControls.Interact;
        pauseMenuPressed = PlayerControls.PauseMenu;
    }
}