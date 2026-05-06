using UnityEngine;

/// <summary>
/// ScriptableObject с настройками игрока для балансировки без изменения кода
/// </summary>
[CreateAssetMenu(fileName = "NewPlayerConfig", menuName = "ScriptableObjects/Player/Player Config")]
public class PlayerConfig : ScriptableObject
{
    [Header("Movement Settings")]
    public float runSpeed = 12f;
    public float walkSpeed = 8f;
    public float crouchSpeed = 5f;
    public float proneSpeed = 3f;
    public float ladderSpeed = 3f;
    public float airControl = 0.4f;
    
    [Header("Jump Settings")]
    public float jumpForce = 8f;
    
    [Header("Pose Settings")]
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float proneHeight = 0.5f;
    public float poseChangeSpeed = 5f;
    
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float healthRegenRate = 5f;
    public float regenDelay = 3f;
    public float regenThreshold = 30f;
    
    [Header("Camera Settings")]
    public float mouseSensitivity = 2f;
    public float aimFOV = 40f;
    public float hipFOV = 60f;
    public float aimTransitionSpeed = 10f;
    
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public float weaponSwapHoldTime = 0.5f;
    
    private void OnValidate()
    {
        // Валидация значений
        runSpeed = Mathf.Max(1f, runSpeed);
        walkSpeed = Mathf.Max(1f, walkSpeed);
        maxHealth = Mathf.Max(1f, maxHealth);
        mouseSensitivity = Mathf.Max(0.1f, mouseSensitivity);
    }
}
