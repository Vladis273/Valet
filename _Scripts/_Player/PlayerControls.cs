using UnityEngine;

public static class PlayerControls
{
    // Ïåðåäâèæåíèå
    public static bool MoveForward => Input.GetKey(KeyCode.W);
    public static bool MoveBack => Input.GetKey(KeyCode.S);
    public static bool MoveLeft => Input.GetKey(KeyCode.A);
    public static bool MoveRight => Input.GetKey(KeyCode.D);
    public static bool Run => Input.GetKey(KeyCode.LeftShift);

    // Ïðûæîê è ïîçû
    public static bool Jump => Input.GetKeyDown(KeyCode.Space);
    public static bool Crouch => Input.GetKeyDown(KeyCode.C);
    public static bool AltCrouch => Input.GetKey(KeyCode.LeftControl);
    public static bool Prone => Input.GetKeyDown(KeyCode.Z);   

    // Îðóæèå è äåéñòâèÿ
    public static bool FireHeld => Input.GetMouseButton(0);
    public static bool FirePressed => Input.GetMouseButtonDown(0);
    public static bool Aim => Input.GetMouseButton(1);
    public static bool Reload => Input.GetKeyDown(KeyCode.R);
    public static bool InspectOrChangeHold => Input.GetKey(KeyCode.LeftAlt);
    public static bool CycleFireMode => Input.GetKeyDown(KeyCode.B);

    // Ñëîòû
    public static bool WeaponSlot1 => Input.GetKeyDown(KeyCode.Alpha1);
    public static bool WeaponSlot2 => Input.GetKeyDown(KeyCode.Alpha2);

    // Ãðàíàòû
    public static bool ThrowGrenade => Input.GetKeyDown(KeyCode.G);
    public static bool CycleGrenade => Input.GetKeyDown(KeyCode.V);

    // Êîìàíäà
    public static bool OpenRadialMenu => Input.GetKeyDown(KeyCode.Tab);

    // Ïðî÷åå
    public static bool PauseMenu => Input.GetKeyDown(KeyCode.Escape);
    public static bool Interact => Input.GetKeyDown(KeyCode.F);
}
