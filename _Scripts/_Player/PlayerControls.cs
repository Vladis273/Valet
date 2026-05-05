using UnityEngine;

public static class PlayerControls
{
    // Передвижение
    public static bool MoveForward => Input.GetKey(KeyCode.W);
    public static bool MoveBack => Input.GetKey(KeyCode.S);
    public static bool MoveLeft => Input.GetKey(KeyCode.A);
    public static bool MoveRight => Input.GetKey(KeyCode.D);
    public static bool Run => Input.GetKey(KeyCode.LeftShift);

    // Прыжок и позы
    public static bool Jump => Input.GetKeyDown(KeyCode.Space); // однократное нажатие
    public static bool Crouch => Input.GetKeyDown(KeyCode.C);   // однократное нажатие
    public static bool AltCrouch => Input.GetKey(KeyCode.LeftControl);// удержание
    public static bool Prone => Input.GetKeyDown(KeyCode.Z);    // однократное нажатие    

    // Оружие и действия
    public static bool FireHeld => Input.GetMouseButton(0);
    public static bool FirePressed => Input.GetMouseButtonDown(0);
    public static bool Aim => Input.GetMouseButton(1);
    public static bool Reload => Input.GetKeyDown(KeyCode.R);   // начало перезарядки
    public static bool InspectOrChangeHold => Input.GetKey(KeyCode.LeftAlt);  // удержание Alt (R зарезервирован для перезарядки)
    public static bool CycleFireMode => Input.GetKeyDown(KeyCode.B);

    // Слоты
    public static bool WeaponSlot1 => Input.GetKeyDown(KeyCode.Alpha1);
    public static bool WeaponSlot2 => Input.GetKeyDown(KeyCode.Alpha2);

    // Гранаты
    public static bool ThrowGrenade => Input.GetKeyDown(KeyCode.G);
    public static bool CycleGrenade => Input.GetKeyDown(KeyCode.V);

    // Команда
    public static bool OpenRadialMenu => Input.GetKeyDown(KeyCode.Tab);

    // Прочее
    public static bool PauseMenu => Input.GetKeyDown(KeyCode.Escape);
    public static bool Interact => Input.GetKeyDown(KeyCode.F);
}