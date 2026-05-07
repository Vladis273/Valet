using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform crosshairParent;
    [SerializeField] private Transform weaponPanel;
    [SerializeField] private Transform grenadePanel;
    [SerializeField] private Transform stancePanel;
    [SerializeField] private Transform notificationArea;
    
    [Header("Weapon UI Elements")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image reloadSliderFill;
    [SerializeField] private GameObject reloadIndicatorObject;
    [SerializeField] private TextMeshProUGUI fireModeText;

    [Header("Grenade UI Elements")]
    [SerializeField] private TextMeshProUGUI grenadeNameText;
    [SerializeField] private TextMeshProUGUI grenadeCountText;
    [SerializeField] private Image grenadeIconImage;

    [Header("Stance UI Elements")]
    [SerializeField] private Image stanceIconImage;
    [SerializeField] private TextMeshProUGUI stanceText;
    
    [Header("Settings")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private float notificationLifetime = 3f;
    
    // Singleton instance
    public static GameUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #region Weapon UI

    public void UpdateWeaponInfo(string weaponName, int currentAmmo, int reserveAmmo, string fireMode)
    {
        if (weaponNameText) weaponNameText.text = weaponName.ToUpper();
        if (ammoText) ammoText.text = $"{currentAmmo} / {reserveAmmo}";
        if (fireModeText) fireModeText.text = fireMode;
    }

    public void ShowReload(float duration)
    {
        if (reloadIndicatorObject) reloadIndicatorObject.SetActive(true);
        if (reloadSliderFill)
        {
            StartCoroutine(AnimateReload(duration));
        }
    }

    public void HideReload()
    {
        if (reloadIndicatorObject) reloadIndicatorObject.SetActive(false);
    }

    private IEnumerator AnimateReload(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            reloadSliderFill.fillAmount = Mathf.Clamp01(timer / duration);
            yield return null;
        }
        reloadSliderFill.fillAmount = 0f;
    }

    #endregion

    #region Grenade UI

    public void UpdateGrenadeInfo(string grenadeName, int count, Sprite icon)
    {
        if (grenadeNameText) grenadeNameText.text = grenadeName.ToUpper();
        if (grenadeCountText) grenadeCountText.text = count.ToString();
        if (grenadeIconImage && icon != null) grenadeIconImage.sprite = icon;
    }

    #endregion

    #region Stance UI

    public void UpdateStanceInfo(PlayerStance stance, Sprite icon)
    {
        string stanceString = stance switch
        {
            PlayerStance.Standing => "STANDING",
            PlayerStance.Crouching => "CROUCHING",
            PlayerStance.Proning => "PRONING",
            _ => "UNKNOWN"
        };

        if (stanceText) stanceText.text = stanceString;
        if (stanceIconImage && icon != null) stanceIconImage.sprite = icon;
    }

    #endregion

    #region Notifications

    public void ShowNotification(string message, Color? color = null)
    {
        if (notificationPrefab == null)
        {
            Debug.LogWarning("Notification prefab not assigned!");
            return;
        }

        GameObject notifObj = Instantiate(notificationPrefab, notificationArea);
        TextMeshProUGUI textComp = notifObj.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp) textComp.text = message;

        if (color.HasValue)
        {
            Image bg = notifObj.GetComponent<Image>();
            if (bg) bg.color = color.Value;
        }

        Destroy(notifObj, notificationLifetime);
    }

    #endregion
    
    // Helper to toggle whole panels if needed
    public void SetWeaponPanelActive(bool active)
    {
        if (weaponPanel) weaponPanel.gameObject.SetActive(active);
    }
    
    public void SetGrenadePanelActive(bool active)
    {
        if (grenadePanel) grenadePanel.gameObject.SetActive(active);
    }
}

// Enum for stances, should match player movement
public enum PlayerStance
{
    Standing,
    Crouching,
    Proning
}
