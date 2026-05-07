using UnityEngine;

/// <summary>
/// Тип магазина для классификации
/// </summary>
public enum MagazineType
{
    Pistol,      // Пистолетные патроны
    Rifle,       // Винтовочные патроны
    Shotgun,     // Дробовые патроны
    SMG,         // Патроны для ПП
    Sniper,      // Снайперские патроны
    Special      // Специальные боеприпасы
}

/// <summary>
/// ScriptableObject с данными магазина для конкретного оружия
/// Каждое оружие имеет свой уникальный тип магазина
/// </summary>
[CreateAssetMenu(fileName = "NewMagazine", menuName = "ScriptableObjects/Weapons/Magazine Data")]
public class MagazineData : ScriptableObject
{
    [Header("General Settings")]
    public string magazineName = "New Magazine";
    public Sprite magazineIcon;
    
    [Header("Ammo Settings")]
    public MagazineType ammoType;
    public int capacity = 30;
    public float reloadTime = 2.5f;
    public float tacticalReloadTime = 1.8f;
    
    [Header("Physical Properties")]
    public float weight = 0.5f;
    public Vector3 size = new Vector3(0.15f, 0.05f, 0.03f);
    
    [Header("Visual")]
    public GameObject magazinePrefab;
    public Material magazineMaterial;
    
    [Header("Audio")]
    public AudioClip insertSound;
    public AudioClip removeSound;
    
    #region Cached Values
    private float _cachedReloadRate;
    private float _cachedTacticalReloadRate;
    
    public float ReloadRate
    {
        get
        {
            if (_cachedReloadRate <= 0f)
                _cachedReloadRate = capacity / reloadTime;
            return _cachedReloadRate;
        }
    }
    
    public float TacticalReloadRate
    {
        get
        {
            if (_cachedTacticalReloadRate <= 0f)
                _cachedTacticalReloadRate = capacity / tacticalReloadTime;
            return _cachedTacticalReloadRate;
        }
    }
    
    private void OnValidate()
    {
        _cachedReloadRate = 0f;
        _cachedTacticalReloadRate = 0f;
        capacity = Mathf.Max(1, capacity);
        reloadTime = Mathf.Max(0.1f, reloadTime);
        tacticalReloadTime = Mathf.Max(0.1f, tacticalReloadTime);
    }
    #endregion
}
