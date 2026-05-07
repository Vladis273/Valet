using UnityEngine;

namespace FollowCamera
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Mouse Look Settings")]
        [SerializeField] private float mouseSensitivityX = 100f;
        [SerializeField] private float mouseSensitivityY = 100f;
        [SerializeField] private bool invertMouseY = false;

        [Header("Camera Constraints")]
        [SerializeField] private float minVerticalAngle = -90f;
        [SerializeField] private float maxVerticalAngle = 90f;
        [SerializeField] private float ladderHorizontalLimit = 75f;
        [SerializeField] private float ladderVerticalLimit = 75f;

        [Header("Smoothing")]
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private bool enableSmoothing = true;

        [Header("References")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private IPlayerStateProvider playerStateProvider;

        [Header("Zoom/Aim Settings")]
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float zoomedFOV = 30f;
        [SerializeField] private float runningFOV = 70f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float aimSensitivityMultiplier = 0.5f;

        [Header("Camera Shake Settings")]
        [SerializeField] private bool enableCameraShake = true;
        [SerializeField] private float walkShakeIntensity = 0.02f;
        [SerializeField] private float runShakeIntensity = 0.05f;
        [SerializeField] private float crouchShakeIntensity = 0.015f;
        [SerializeField] private float proneShakeIntensity = 0.01f;
        [SerializeField] private float shakeFrequency = 10f;
        [SerializeField] private float landingShakeIntensity = 0.15f;
        [SerializeField] private float landingShakeDuration = 0.3f;
        [SerializeField] private float shakeReduction = 2f;
        
        [Header("Recoil Camera Settings")]
        [SerializeField] private float recoilKickAmount = 0.1f;
        [SerializeField] private float recoilRotationAmount = 0.5f;
        [SerializeField] private float recoilLerpSpeed = 8f;

        private float xRotation = 0f;
        private Vector2 currentMouseDelta;
        private Vector2 currentMouseDeltaVelocity;

        private bool isAiming = false;
        private float targetFOV;
        private float currentFOV;

        [Header("Camera Recoil Settings")]
        private Vector3 recoilOffset;
        private Vector3 recoilVelocity;
        private float targetRecoilRotationX;
        private float currentRecoilRotationX;
        [SerializeField] private float recoilReturnSpeed = 6f;
        [SerializeField] private float recoilDamping = 10f;

        private Vector3 defaultLocalPosition;
        private Vector3 cameraShakeOffset;
        private float shakeTimer;
        private float landingShakeTimer;
        private bool wasGrounded = true;

        private PlayerMovement playerMovement;
        private PlayerInput playerInput;
        private float ladderYaw = 0f;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            playerInput = GetComponentInParent<PlayerInput>();
            playerCamera = GetComponent<Camera>();
            
            if (playerBody == null)
            {
                playerBody = transform.parent;
                Debug.LogWarning("[CameraFollow] playerBody was not assigned, using transform.parent as fallback.");
            }
            
            playerStateProvider = playerBody.GetComponent<IPlayerStateProvider>();
            playerMovement = playerBody.GetComponent<PlayerMovement>();

            defaultLocalPosition = transform.localPosition;
            xRotation = transform.localEulerAngles.x;

            normalFOV = playerCamera.fieldOfView;
            currentFOV = normalFOV;
            targetFOV = normalFOV;
        }

        void Update()
        {
            HandleMouseLook();
            HandleZoom();
            HandleCameraShake();
            HandleCursorToggle();
        }

        private void HandleMouseLook()
        {
            if (Cursor.visible) return;
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;

            bool onLadder = playerMovement != null && playerMovement.IsOnLadder();

            if (isAiming)
            {
                mouseX *= aimSensitivityMultiplier;
                mouseY *= aimSensitivityMultiplier;
            }

            if (invertMouseY)
                mouseY = -mouseY;

            if (enableSmoothing)
            {
                Vector2 targetMouseDelta = new Vector2(mouseX, mouseY);
                currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, smoothTime);
                mouseX = currentMouseDelta.x;
                mouseY = currentMouseDelta.y;
            }

            xRotation -= mouseY;
            if (onLadder)
            {
                xRotation = Mathf.Clamp(xRotation, -ladderVerticalLimit, ladderVerticalLimit);
            }
            else
            {
                xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);
            }

            if (playerBody != null)
            {
                if (onLadder)
                {
                    ladderYaw += mouseX;
                    ladderYaw = Mathf.Clamp(ladderYaw, -ladderHorizontalLimit, ladderHorizontalLimit);
                    transform.localRotation = Quaternion.Euler(xRotation, ladderYaw, 0f);
                }
                else
                {
                    playerBody.Rotate(0f, mouseX, 0f);
                    transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                    ladderYaw = 0f;
                }
            }
            else
            {
                transform.Rotate(-mouseY, mouseX, 0f);
                return;
            }

            if (enableCameraShake)
                transform.localPosition = cameraShakeOffset;
        }

        private void HandleZoom()
        {
            isAiming = playerInput.aimHeld;

            bool isPlayerRunning = playerStateProvider != null && playerStateProvider.IsRunning();

            if (isAiming)
                targetFOV = zoomedFOV;
            else if (isPlayerRunning)
                targetFOV = runningFOV;
            else
                targetFOV = normalFOV;

            currentFOV = Mathf.Lerp(currentFOV, targetFOV, zoomSpeed * Time.deltaTime);

            if (playerCamera != null)
                playerCamera.fieldOfView = currentFOV;
        }

        private void HandleCameraShake()
        {
            if (!enableCameraShake || playerStateProvider == null)
            {
                transform.localPosition = defaultLocalPosition;
                transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                return;
            }

            bool isGrounded = playerStateProvider.IsGrounded();
            bool isMoving = playerStateProvider.IsMoving();
            bool isRunning = playerStateProvider.IsRunning();
            PlayerPose currentPose = playerStateProvider.GetCurrentPose();

            if (isGrounded && !wasGrounded)
                landingShakeTimer = landingShakeDuration;

            wasGrounded = isGrounded;

            float shakeIntensity = 0f;

            if (landingShakeTimer > 0f)
            {
                landingShakeTimer -= Time.deltaTime;
                float landingShakeAmount = (landingShakeTimer / landingShakeDuration) * landingShakeIntensity;
                shakeIntensity = Mathf.Max(shakeIntensity, landingShakeAmount);
            }

            if (isMoving && isGrounded)
            {
                float movementShake = GetShakeForPose(currentPose, isRunning);
                shakeIntensity = Mathf.Max(shakeIntensity, movementShake);
            }

            if (isAiming)
                shakeIntensity *= aimSensitivityMultiplier;

            if (shakeIntensity > 0f)
            {
                shakeTimer += Time.deltaTime * shakeFrequency;
                float shakeX = Mathf.Sin(shakeTimer) * shakeIntensity;
                float shakeY = Mathf.Sin(shakeTimer * 1.3f) * shakeIntensity * 0.7f;
                float shakeZ = Mathf.Sin(shakeTimer * 0.8f) * shakeIntensity * 0.5f;
                cameraShakeOffset = new Vector3(shakeX, shakeY, shakeZ);
            }
            else
            {
                cameraShakeOffset = Vector3.Lerp(cameraShakeOffset, Vector3.zero, Time.deltaTime * shakeReduction);
            }

            // Обработка отдачи камеры
            Vector3 spring = -recoilOffset * recoilReturnSpeed;
            Vector3 damping = -recoilVelocity * recoilDamping;
            recoilVelocity += (spring + damping) * Time.deltaTime;
            recoilOffset += recoilVelocity * Time.deltaTime;

            // Плавное возвращение вращения от отдачи
            currentRecoilRotationX = Mathf.Lerp(currentRecoilRotationX, targetRecoilRotationX, Time.deltaTime * recoilLerpSpeed);
            if (Mathf.Abs(targetRecoilRotationX) < 0.01f)
                targetRecoilRotationX = 0f;
            if (Mathf.Abs(currentRecoilRotationX) < 0.01f)
                currentRecoilRotationX = 0f;

            Vector3 totalShake = cameraShakeOffset + recoilOffset;

            transform.localPosition = defaultLocalPosition + totalShake;
            
            // Применяем вращение от отдачи к камере
            if (currentRecoilRotationX != 0f)
            {
                transform.localRotation = Quaternion.Euler(xRotation + currentRecoilRotationX, 0f, 0f);
            }
        }

        private float GetShakeForPose(PlayerPose pose, bool isRunning)
        {
            float baseShake = isRunning ? runShakeIntensity : walkShakeIntensity;
            
            // Уменьшаем тряску для приседа и положения лежа
            switch (pose)
            {
                case PlayerPose.Crouch:
                case PlayerPose.AltCrouch:
                    return baseShake * (crouchShakeIntensity / walkShakeIntensity);
                case PlayerPose.Prone:
                    return baseShake * (proneShakeIntensity / walkShakeIntensity);
                default:
                    return baseShake;
            }
        }

        private void HandleCursorToggle()
        {
            if (playerInput.openRadialMenuPressed)
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                    ? CursorLockMode.None
                    : CursorLockMode.Locked;
                Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
            }
        }

        public void ApplyRecoilImpulse(Vector2 impulse)
        {
            // Вертикальная отдача - камера поднимается вверх
            recoilVelocity.y += impulse.y * recoilKickAmount;
            
            // Горизонтальная случайная отдача
            recoilVelocity.x += (Random.value > 0.5f ? 1f : -1f) * impulse.x * 0.5f;
            
            // Вращение камеры от отдачи (кик вверх)
            targetRecoilRotationX -= impulse.y * recoilRotationAmount;
        }

        public bool IsAiming() => isAiming;
    }
}