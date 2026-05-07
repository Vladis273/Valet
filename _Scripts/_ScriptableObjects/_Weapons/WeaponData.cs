using UnityEngine;

/// <summary>
/// Перечисление режимов огня (вынесено из WeaponController для использования в ScriptableObjects)
/// </summary>
public enum FireMode { Single, Burst, Auto }

/// <summary>
/// ScriptableObject с данными оружия для настройки префабов и баланса
/// </summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "ScriptableObjects/Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("General Settings")]
    public string weaponName = "New Weapon";
    public Sprite weaponIcon;
    
    [Header("Hit-Scan Settings")]
    public float range = 100f;
    public float damage = 30f;
    
    [Tooltip("Скорострельность (выстрелов в минуту)")]
    public float fireRate = 600f;
    
    [Header("Recoil Settings")]
    public Vector2 recoilImpulse = new Vector2(0.5f, 1f);
    public float recoilDuration = 0.15f;
    public float maxArmRecoilOffset = 0.4f;
    
    [Header("Fire Mode")]
    public FireMode defaultFireMode = FireMode.Single;
    public FireMode[] availableFireModes = new FireMode[] { FireMode.Single };
    public int burstLength = 3;
    public float burstDuration = 0.5f;
    
    [Header("Spread System")]
    public int accurateShots = 3;
    public float maxSpread = 5f;
    public float spreadIncreaseRate = 0.5f;
    public float spreadRecoveryTime = 1f;
    
    [Header("Shotgun Settings")]
    public bool isShotgun = false;
    public int pelletsPerShot = 8;
    public float shotgunSpread = 15f;
    
    [Header("Aiming")]
    public float aimAccurateShotsMultiplier = 2f;
    [Range(0f, 1f)]
    public float aimMovementSlowdown = 0.5f;
    public float aimSpreadReduction = 0.5f;
    
    [Header("Reload")]
    public float reloadTimeFull = 2.5f;
    public float reloadTimeTactical = 1.8f;
    
    [Header("Ammo")]
    public MagazineData magazineData;
    public int maxReserveAmmo = 120;
    
    [Header("Reload (Override)")]
    [Tooltip("Если не задано, используются значения из MagazineData")]
    public float reloadTimeFullOverride = 0f;
    [Tooltip("Если не задано, используются значения из MagazineData")]
    public float reloadTimeTacticalOverride = 0f;
    
    [Header("Legacy Support")]
    [Tooltip("Устаревшее поле, используется только если magazineData не задан")]
    public int legacyMagazineCapacity = 30;
    
    [Header("UI")]
    public float hintDisplayTime = 2f;
    
    // Кэшированные значения для производительности
    private float _cachedFireInterval;
    private float _cachedBurstInterval;
    
    public float FireInterval
    {
        get
        {
            if (_cachedFireInterval <= 0f)
                _cachedFireInterval = 60f / fireRate;
            return _cachedFireInterval;
        }
    }
    
    public float BurstInterval
    {
        get
        {
            if (_cachedBurstInterval <= 0f)
                _cachedBurstInterval = burstDuration / Mathf.Max(1, burstLength);
            return _cachedBurstInterval;
        }
    }
    
    private void OnValidate()
    {
        // Сброс кэша при изменении значений в редакторе
        _cachedFireInterval = 0f;
        _cachedBurstInterval = 0f;
    }
}
