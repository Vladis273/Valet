using UnityEngine;

/// <summary>
/// Editor-скрипт для создания пресетов WeaponData через код
/// Запустить из меню: Tools > Create Weapon Presets > Create All Weapons
/// </summary>
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public class WeaponDataCreator : EditorWindow
{
    [MenuItem("Tools/Create Weapon Presets/Create All Weapons")]
    public static void CreateAllWeapons()
    {
        string basePath = "Assets/_ScriptableObjects/Weapons/Configs";
        
        // Создаем директорию если не существует
        if (!AssetDatabase.IsValidFolder(basePath))
        {
            string[] folders = basePath.Split('/');
            string currentPath = "Assets";
            for (int i = 1; i < folders.Length; i++)
            {
                currentPath += "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(currentPath))
                {
                    AssetDatabase.CreateFolder(currentPath.Substring(0, currentPath.LastIndexOf('/')), folders[i]);
                }
            }
        }

        // Загружаем пресеты магазинов
        MagazineData hk65Mag = AssetDatabase.LoadAssetAtPath<MagazineData>(basePath + "/../Magazines/HK65_Magazine.asset");
        MagazineData umpMag = AssetDatabase.LoadAssetAtPath<MagazineData>(basePath + "/../Magazines/UMP_Magazine.asset");
        MagazineData akmMag = AssetDatabase.LoadAssetAtPath<MagazineData>(basePath + "/../Magazines/AKM_Magazine.asset");
        MagazineData augMag = AssetDatabase.LoadAssetAtPath<MagazineData>(basePath + "/../Magazines/AUG_Magazine.asset");
        MagazineData cz907Mag = AssetDatabase.LoadAssetAtPath<MagazineData>(basePath + "/../Magazines/CZ907_Magazine.asset");
        MagazineData m16Mag = AssetDatabase.LoadAssetAtPath<MagazineData>(basePath + "/../Magazines/M16_Magazine.asset");
        MagazineData pumpShotgunMag = AssetDatabase.LoadAssetAtPath<MagazineData>(basePath + "/../Magazines/PumpShotgun_Shells.asset");
        MagazineData benelliM4Mag = AssetDatabase.LoadAssetAtPath<MagazineData>(basePath + "/../Magazines/BenelliM4_Magazine.asset");

        // HK65 Pistol
        CreateWeapon(
            basePath,
            "HK65_Pistol",
            "HK65",
            damage: 35f,
            fireRate: 400f,
            range: 50f,
            recoilImpulse: new Vector2(0.3f, 0.8f),
            magazineData: hk65Mag,
            maxReserveAmmo: 90,
            isShotgun: false,
            fireMode: FireMode.Single
        );

        // UMP SMG
        CreateWeapon(
            basePath,
            "UMP_SMG",
            "UMP",
            damage: 28f,
            fireRate: 650f,
            range: 80f,
            recoilImpulse: new Vector2(0.4f, 0.6f),
            magazineData: umpMag,
            maxReserveAmmo: 150,
            isShotgun: false,
            fireMode: FireMode.Auto
        );

        // AKM Assault Rifle
        CreateWeapon(
            basePath,
            "AKM_Assault",
            "AKM",
            damage: 32f,
            fireRate: 600f,
            range: 150f,
            recoilImpulse: new Vector2(0.6f, 1.2f),
            magazineData: akmMag,
            maxReserveAmmo: 150,
            isShotgun: false,
            fireMode: FireMode.Auto
        );

        // AUG Assault Rifle
        CreateWeapon(
            basePath,
            "AUG_Assault",
            "AUG",
            damage: 30f,
            fireRate: 680f,
            range: 160f,
            recoilImpulse: new Vector2(0.5f, 1.0f),
            magazineData: augMag,
            maxReserveAmmo: 150,
            isShotgun: false,
            fireMode: FireMode.Auto
        );

        // CZ-907 Assault Rifle
        CreateWeapon(
            basePath,
            "CZ907_Assault",
            "CZ-907",
            damage: 31f,
            fireRate: 620f,
            range: 155f,
            recoilImpulse: new Vector2(0.55f, 1.1f),
            magazineData: cz907Mag,
            maxReserveAmmo: 150,
            isShotgun: false,
            fireMode: FireMode.Auto
        );

        // M16 Assault Rifle (Burst + Single)
        CreateWeapon(
            basePath,
            "M16_Assault",
            "M16",
            damage: 30f,
            fireRate: 700f,
            range: 170f,
            recoilImpulse: new Vector2(0.5f, 1.0f),
            magazineData: m16Mag,
            maxReserveAmmo: 150,
            isShotgun: false,
            fireMode: FireMode.Burst,
            burstLength: 3,
            availableFireModes: new FireMode[] { FireMode.Single, FireMode.Burst }
        );

        // Pump Shotgun (Помповый дробовик)
        CreateWeapon(
            basePath,
            "PumpShotgun",
            "Pump Shotgun",
            damage: 25f, // Урон одной дробины
            fireRate: 60f, // Медленная перезарядка между выстрелами
            range: 40f,
            recoilImpulse: new Vector2(1.0f, 2.0f),
            magazineData: pumpShotgunMag,
            maxReserveAmmo: 60,
            isShotgun: true,
            pelletsPerShot: 8,
            shotgunSpread: 20f,
            fireMode: FireMode.Single
        );

        // Benelli M4 (Автодробовик)
        CreateWeapon(
            basePath,
            "BenelliM4",
            "Benelli M4",
            damage: 24f, // Урон одной дробины
            fireRate: 120f, // Быстрее помпового
            range: 45f,
            recoilImpulse: new Vector2(0.9f, 1.8f),
            magazineData: benelliM4Mag,
            maxReserveAmmo: 60,
            isShotgun: true,
            pelletsPerShot: 8,
            shotgunSpread: 18f,
            fireMode: FireMode.Auto
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✓ Создано 8 пресетов WeaponData в {basePath}");
        EditorUtility.DisplayDialog("Готово", $"Создано 8 пресетов оружия:\n- HK65 (Пистолет)\n- UMP (ПП)\n- AKM, AUG, CZ-907, M16 (Автоматы)\n- Pump Shotgun (Дробовик)\n- Benelli M4 (Автодробовик)", "OK");
    }

    private static void CreateWeapon(string basePath, string fileName, string displayName,
        float damage, float fireRate, float range, Vector2 recoilImpulse,
        MagazineData magazineData, int maxReserveAmmo, bool isShotgun,
        FireMode fireMode, int pelletsPerShot = 1, float shotgunSpread = 0f,
        int burstLength = 3, FireMode[] availableFireModes = null)
    {
        WeaponData weapon = ScriptableObject.CreateInstance<WeaponData>();
        weapon.name = displayName;
        weapon.weaponName = displayName;
        weapon.damage = damage;
        weapon.fireRate = fireRate;
        weapon.range = range;
        weapon.recoilImpulse = recoilImpulse;
        weapon.magazineData = magazineData;
        weapon.maxReserveAmmo = maxReserveAmmo;
        weapon.isShotgun = isShotgun;
        weapon.defaultFireMode = fireMode;
        
        if (availableFireModes != null)
        {
            weapon.availableFireModes = availableFireModes;
        }
        else
        {
            weapon.availableFireModes = new FireMode[] { fireMode };
        }
        
        weapon.burstLength = burstLength;
        
        if (isShotgun)
        {
            weapon.pelletsPerShot = pelletsPerShot;
            weapon.shotgunSpread = shotgunSpread;
        }
        
        string fullPath = Path.Combine(basePath, fileName + ".asset");
        AssetDatabase.CreateAsset(weapon, fullPath);
        Debug.Log($"Создано оружие: {fullPath}");
    }
}
#endif
