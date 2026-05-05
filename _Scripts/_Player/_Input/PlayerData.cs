using FollowCamera;
using LightSide;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [Header("Player")]
    public PlayerInput playerInput;
    public PlayerMovement playerMovement;
    public WeaponInventory weaponInventory;

    [Header("Arms")]
    public GameObject gunContainer;
    public GameObject ladderArms;

    [Header("Camera")]
    public CameraFollow cameraFollow;
    public Transform cameraTransform;
    public Camera playerCamera;

    [Header("Text")]
    public UniText interactHint;
    public UniText fireModeHint;
    public UniText ammoHint;
}
