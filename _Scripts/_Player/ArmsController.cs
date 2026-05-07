using UnityEngine;

public class ArmsController : MonoBehaviour
{
    [Header("Settings")]
    public PlayerData playerData;
    public GameObject dropPrefab;
    public WeaponController weaponController;

    [Header("Rotation Lag")]
    public float rotationLagSpeed = 8f;

    [Header("Movement Rotation Lag")]
    public float dynamicArmsRotationAmount = 3f;
    public float dynamicArmsRotationSpeed = 5f;
    public float dynamicArmsReturnSpeed = 4f;

    [Header("Jump Sway")]
    public float jumpSwayAmount = 1.5f;
    public float jumpSwaySpeed = 3f;

    [Header("Arm Position")]
    public Vector3 armsOffset;
    public Vector3 ladderArmsOffset;

    [Header("Arm Aim Settings")]
    public Vector3 aimOffset;
    public float aimBlendSpeed = 10f;

    [Header("Recoil Settings")]
    public float recoilReturnSpeed = 8f;
    public float recoilDamping = 12f;
    [SerializeField] private float maxRecoilHeight = 0.4f;

    [Header("Recoil Rotation")]
    public float recoilRotationStrength = 15f;
    public float recoilRotationReturnSpeed = 8f;
    public float recoilRotationDamping = 10f;

    #region Private
    private float dynamicArmsRotationTarget;
    private float dynamicArmsRotationCurrent;

    private Vector3 recoilRotationVelocity;
    private Vector3 recoilRotationPosition;

    private Vector3 recoilVelocity;
    private Vector3 recoilPosition;

    private Vector3 movementSway;

    private Quaternion targetRotation;
    private Quaternion currentRotation;

    private float jumpSway;
    private float aimBlend;
    private float currentPoseOffset;

    private Rigidbody rb;
    private PlayerMovement playerMovement;
    private Transform cameraFollowTransform;
    [HideInInspector] public Animator animator;
    private FollowCamera.CameraFollow cameraFollow;
    
    // Кэшированные значения для оптимизации
    private Vector3 _cachedHorizontalVelocity;
    private bool _wasGrounded;
    private float _lastAimBlend;
    #endregion

    void Start()
    {
        weaponController = GetComponentInChildren<WeaponComponentsTransform>().controller;
        playerMovement = playerData.playerMovement;
        cameraFollowTransform = playerData.cameraTransform;
        cameraFollow = playerData.cameraFollow;
        rb = playerMovement.GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        currentRotation = cameraFollowTransform.rotation;
        targetRotation = currentRotation;
        
        weaponController.enabled = true;
        
        _cachedHorizontalVelocity = Vector3.zero;
        _wasGrounded = true;
        _lastAimBlend = 0f;
    }

    void OnEnable()
    {
        if (cameraFollowTransform != null)
        {
            currentRotation = cameraFollowTransform.rotation;
            targetRotation = currentRotation;
            transform.rotation = currentRotation;
        }
    }

    void Update()
    {
        bool isAiming = cameraFollow.IsAiming();
        
        // Оптимизация: обновляем aimBlend только если изменилось состояние прицеливания
        if (isAiming ? _lastAimBlend < 0.99f : _lastAimBlend > 0.01f)
        {
            float targetAim = isAiming ? 1f : 0f;
            aimBlend = Mathf.MoveTowards(aimBlend, targetAim, Time.deltaTime * aimBlendSpeed);
            _lastAimBlend = aimBlend;
        }

        Vector3 currentVelocity = rb != null ? rb.linearVelocity : Vector3.zero;
        
        // Оптимизация: обновляем sway только при изменении скорости
        bool velocityChanged = Vector3.SqrMagnitude(currentVelocity - _cachedHorizontalVelocity) > 0.01f;
        if (velocityChanged)
        {
            _cachedHorizontalVelocity = currentVelocity;
            CalculateDynamicArmsRotation(currentVelocity);
        }
        
        bool isGrounded = playerMovement.IsGrounded();
        if (isGrounded != _wasGrounded || velocityChanged)
        {
            _wasGrounded = isGrounded;
            CalculateJumpSway(currentVelocity.y, isGrounded);
        }

        // Объединяем вычисления поворота
        float totalYaw = movementSway.y + dynamicArmsRotationCurrent;
        float totalPitch = movementSway.x + jumpSway;
        float totalRoll = movementSway.z;
        
        Quaternion swayRotation = Quaternion.Euler(totalPitch, totalYaw, totalRoll);

        // Оптимизированная физика отдачи (объединённые вычисления)
        if (recoilPosition.sqrMagnitude > 0.0001f || recoilVelocity.sqrMagnitude > 0.0001f)
        {
            Vector3 springForce = -recoilPosition * recoilReturnSpeed;
            Vector3 dampingForce = -recoilVelocity * recoilDamping;
            recoilVelocity += (springForce + dampingForce) * Time.deltaTime;
            recoilPosition += recoilVelocity * Time.deltaTime;
            recoilPosition.y = Mathf.Clamp(recoilPosition.y, 0f, maxRecoilHeight);
        }
        else
        {
            recoilPosition = Vector3.zero;
            recoilVelocity = Vector3.zero;
        }

        if (recoilRotationPosition.sqrMagnitude > 0.0001f || recoilRotationVelocity.sqrMagnitude > 0.0001f)
        {
            Vector3 rotSpring = -recoilRotationPosition * recoilRotationReturnSpeed;
            Vector3 rotDamping = -recoilRotationVelocity * recoilRotationDamping;
            recoilRotationVelocity += (rotSpring + rotDamping) * Time.deltaTime;
            recoilRotationPosition += recoilRotationVelocity * Time.deltaTime;
            recoilRotationPosition.x = Mathf.Min(recoilRotationPosition.x, 0f);
        }
        else
        {
            recoilRotationPosition = Vector3.zero;
            recoilRotationVelocity = Vector3.zero;
        }

        // Финальная композиция вращения
        Quaternion finalRotation = currentRotation * swayRotation;
        transform.rotation = finalRotation * Quaternion.Euler(recoilRotationPosition);

        // Плавное следование за камерой
        targetRotation = cameraFollowTransform.rotation;
        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationLagSpeed * Time.deltaTime);

        // Позиция рук
        Vector3 effectiveAimOffset = aimOffset;
        effectiveAimOffset.x = 0f;

        Vector3 currentArmsOffset = Vector3.Lerp(armsOffset, effectiveAimOffset, aimBlend);
        Vector3 finalArmsOffset = currentArmsOffset + recoilPosition;
        transform.position = cameraFollowTransform.position + 
                            cameraFollowTransform.TransformDirection(finalArmsOffset) + 
                            cameraFollowTransform.up * currentPoseOffset + 
                            movementSway * 0.1f;
    }

    void CalculateJumpSway(float verticalVelocity, bool isGrounded)
    {
        float targetJumpSway = (!isGrounded && verticalVelocity > 0.1f) ? jumpSwayAmount : 0f;
        jumpSway = Mathf.MoveTowards(jumpSway, targetJumpSway, Time.deltaTime * jumpSwaySpeed);
    }

    void CalculateDynamicArmsRotation(Vector3 currentVelocity)
    {
        Vector3 horizontalVelocity = currentVelocity;
        horizontalVelocity.y = 0f;

        if (horizontalVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 moveDirection = horizontalVelocity.normalized;
            Vector3 localMoveDir = cameraFollowTransform.InverseTransformDirection(moveDirection);

            dynamicArmsRotationTarget = Mathf.Clamp(
                -localMoveDir.x * dynamicArmsRotationAmount,
                -dynamicArmsRotationAmount, 
                dynamicArmsRotationAmount
            );
        }
        else
        {
            dynamicArmsRotationTarget = 0f;
        }

        float speed = Mathf.Abs(dynamicArmsRotationTarget) > 0.1f ?
            dynamicArmsRotationSpeed : dynamicArmsReturnSpeed;

        dynamicArmsRotationCurrent = Mathf.MoveTowards(dynamicArmsRotationCurrent,
            dynamicArmsRotationTarget, Time.deltaTime * speed);
    }

    public void ApplyRecoilImpulse(Vector2 impulse, float duration, AnimationCurve curve = null)
    {
        recoilVelocity.y += impulse.y;
        recoilVelocity.z -= impulse.x;
        recoilRotationVelocity.x -= impulse.y * recoilRotationStrength;
    }
}