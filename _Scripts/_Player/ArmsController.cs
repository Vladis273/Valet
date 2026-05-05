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

        Vector3 currentVelocity = rb != null ? rb.linearVelocity : Vector3.zero;
        aimBlend = Mathf.Lerp(aimBlend, isAiming ? 1f : 0f, Time.deltaTime * aimBlendSpeed);

        CalculateJumpSway(rb);
        CalculateDynamicArmsRotation();

        Quaternion swayRotation = Quaternion.Euler(
            movementSway.x + jumpSway,
            movementSway.y + dynamicArmsRotationCurrent,
            movementSway.z
        );

        Vector3 targetRecoil = Vector3.zero;
        Vector3 springForce = (targetRecoil - recoilPosition) * recoilReturnSpeed;
        Vector3 dampingForce = -recoilVelocity * recoilDamping;

        recoilVelocity += (springForce + dampingForce) * Time.deltaTime;
        recoilPosition += recoilVelocity * Time.deltaTime;

        recoilPosition.y = Mathf.Clamp(recoilPosition.y, 0f, maxRecoilHeight);

        Vector3 targetRotRecoil = Vector3.zero;
        Vector3 rotSpring = (targetRotRecoil - recoilRotationPosition) * recoilRotationReturnSpeed;
        Vector3 rotDamping = -recoilRotationVelocity * recoilRotationDamping;

        recoilRotationVelocity += (rotSpring + rotDamping) * Time.deltaTime;
        recoilRotationPosition += recoilRotationVelocity * Time.deltaTime;

        recoilRotationPosition.x = Mathf.Min(recoilRotationPosition.x, 0f);

        Quaternion finalRotation = currentRotation * swayRotation;
        Quaternion recoilRotation = Quaternion.Euler(recoilRotationPosition);
        transform.rotation = finalRotation * recoilRotation;

        targetRotation = cameraFollowTransform.rotation;
        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationLagSpeed * Time.deltaTime);

        Vector3 effectiveAimOffset = aimOffset;
        effectiveAimOffset.x = 0f;

        Vector3 currentArmsOffset = Vector3.Lerp(armsOffset, effectiveAimOffset, aimBlend);
        Vector3 finalArmsOffset = currentArmsOffset + recoilPosition;
        Vector3 basePosition = cameraFollowTransform.position + cameraFollowTransform.TransformDirection(finalArmsOffset);
        Vector3 poseOffsetVector = cameraFollowTransform.up * currentPoseOffset;

        transform.position = basePosition + poseOffsetVector + movementSway * 0.1f;

    }

    void CalculateJumpSway(Rigidbody rb)
    {
        bool isGrounded = playerMovement.IsGrounded();

        if (!isGrounded && rb.linearVelocity.y > 0.1f)
            jumpSway = Mathf.Lerp(jumpSway, jumpSwayAmount, Time.deltaTime * jumpSwaySpeed);
        else
            jumpSway = Mathf.Lerp(jumpSway, 0f, Time.deltaTime * jumpSwaySpeed);
    }

    void CalculateDynamicArmsRotation()
    {
        Vector3 horizontalVelocity = rb != null ? rb.linearVelocity : Vector3.zero;
        horizontalVelocity.y = 0f;

        if (horizontalVelocity.magnitude > 0.1f)
        {
            Vector3 moveDirection = horizontalVelocity.normalized;
            Vector3 localMoveDir = cameraFollowTransform.InverseTransformDirection(moveDirection);

            dynamicArmsRotationTarget = -localMoveDir.x * dynamicArmsRotationAmount;

            dynamicArmsRotationTarget = Mathf.Clamp(dynamicArmsRotationTarget,
                -dynamicArmsRotationAmount, dynamicArmsRotationAmount);
        }
        else
        {
            dynamicArmsRotationTarget = 0f;
        }

        float speed = Mathf.Abs(dynamicArmsRotationTarget) > 0.1f ?
            dynamicArmsRotationSpeed : dynamicArmsReturnSpeed;

        dynamicArmsRotationCurrent = Mathf.Lerp(dynamicArmsRotationCurrent,
            dynamicArmsRotationTarget, Time.deltaTime * speed);
    }

    public void ApplyRecoilImpulse(Vector2 impulse, float duration, AnimationCurve curve = null)
    {
        recoilVelocity.y += impulse.y;
        recoilVelocity.z -= impulse.x;
        recoilRotationVelocity.x -= impulse.y * recoilRotationStrength;
    }
}