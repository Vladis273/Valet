using UnityEngine;
using UniText;

/// <summary>
/// Менеджер игрового UI. Управляет обновлением всех элементов интерфейса:
/// оружие, магазины, гранаты, поза игрока и уведомления.
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("Weapon Panel")]
    [SerializeField] private UniText _weaponNameText;
    [SerializeField] private UniText _ammoText;
    [SerializeField] private UniText _fireModeText;
    [SerializeField] private GameObject _reloadBarObject;
    [SerializeField] private RectTransform _reloadBarFill;

    [Header("Grenade Panel")]
    [SerializeField] private UniText _grenadeTypeText;
    [SerializeField] private UniText _grenadeCountText;

    [Header("Stance Panel")]
    [SerializeField] private UniText _stanceText;

    [Header("Notification Area")]
    [SerializeField] private Transform _notificationParent;
    [SerializeField] private GameObject _notificationPrefab;
    [SerializeField] private int _maxNotifications = 5;

    private static GameUIManager _instance;
    public static GameUIManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    #region Weapon UI

    public void UpdateWeaponInfo(string weaponName, int currentAmmo, int reserveAmmo, string fireMode)
    {
        if (_weaponNameText != null)
            _weaponNameText.Text = weaponName;

        if (_ammoText != null)
            _ammoText.Text = $"{currentAmmo} / {reserveAmmo}";

        if (_fireModeText != null)
            _fireModeText.Text = fireMode;
    }

    public void ShowReload(bool isReloading, float duration = 0f)
    {
        if (_reloadBarObject == null) return;

        _reloadBarObject.SetActive(isReloading);

        if (isReloading && duration > 0f && _reloadBarFill != null)
        {
            StartCoroutine(AnimateReloadBar(duration));
        }
    }

    private System.Collections.IEnumerator AnimateReloadBar(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            if (_reloadBarFill != null)
                _reloadBarFill.localScale = new Vector3(progress, 1f, 1f);
            yield return null;
        }
    }

    #endregion

    #region Grenade UI

    public void UpdateGrenadeInfo(string grenadeType, int count)
    {
        if (_grenadeTypeText != null)
            _grenadeTypeText.Text = grenadeType;

        if (_grenadeCountText != null)
            _grenadeCountText.Text = count.ToString();
    }

    #endregion

    #region Stance UI

    public void UpdateStance(string stance)
    {
        if (_stanceText != null)
            _stanceText.Text = stance;
    }

    #endregion

    #region Notifications

    public void ShowNotification(string message, Color? color = null, float duration = 2f)
    {
        if (_notificationParent == null || _notificationPrefab == null)
            return;

        // Удаляем старые уведомления, если превышен лимит
        while (_notificationParent.childCount >= _maxNotifications)
        {
            Destroy(_notificationParent.GetChild(0).gameObject);
        }

        GameObject notificationObj = Instantiate(_notificationPrefab, _notificationParent);
        NotificationItem notification = notificationObj.GetComponent<NotificationItem>();
        
        if (notification != null)
        {
            notification.Initialize(message, color ?? Color.white, duration);
        }
    }

    #endregion
}
