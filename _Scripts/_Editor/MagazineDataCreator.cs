using UnityEngine;

/// <summary>
/// Editor-скрипт для создания пресетов MagazineData через код
/// Запустить из меню: Tools > Create Weapon Presets > Create All Magazines
/// </summary>
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public class MagazineDataCreator : EditorWindow
{
    [MenuItem("Tools/Create Weapon Presets/Create All Magazines")]
    public static void CreateAllMagazines()
    {
        string basePath = "Assets/_ScriptableObjects/Weapons/Magazines";
        
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

        // HK65 Pistol Magazine (9x19mm)
        CreateMagazine(
            basePath,
            "HK65_Magazine",
            "HK65 9x19mm Magazine",
            MagazineType.Pistol,
            capacity: 15,
            reloadTime: 2.0f,
            tacticalReloadTime: 1.5f,
            weight: 0.3f
        );

        // UMP SMG Magazine (.45 ACP)
        CreateMagazine(
            basePath,
            "UMP_Magazine",
            "UMP .45 ACP Magazine",
            MagazineType.SMG,
            capacity: 25,
            reloadTime: 2.2f,
            tacticalReloadTime: 1.6f,
            weight: 0.4f
        );

        // AKM Magazine (7.62x39mm)
        CreateMagazine(
            basePath,
            "AKM_Magazine",
            "AKM 7.62x39mm Magazine",
            MagazineType.Rifle,
            capacity: 30,
            reloadTime: 2.5f,
            tacticalReloadTime: 1.8f,
            weight: 0.5f
        );

        // AUG Magazine (5.56x45mm)
        CreateMagazine(
            basePath,
            "AUG_Magazine",
            "AUG 5.56x45mm Magazine",
            MagazineType.Rifle,
            capacity: 30,
            reloadTime: 2.3f,
            tacticalReloadTime: 1.7f,
            weight: 0.45f
        );

        // CZ-907 Magazine (5.45x39mm или 5.56x45mm)
        CreateMagazine(
            basePath,
            "CZ907_Magazine",
            "CZ-907 5.45x39mm Magazine",
            MagazineType.Rifle,
            capacity: 30,
            reloadTime: 2.4f,
            tacticalReloadTime: 1.75f,
            weight: 0.45f
        );

        // M16 Magazine (5.56x45mm)
        CreateMagazine(
            basePath,
            "M16_Magazine",
            "M16 5.56x45mm Magazine",
            MagazineType.Rifle,
            capacity: 30,
            reloadTime: 2.3f,
            tacticalReloadTime: 1.7f,
            weight: 0.45f
        );

        // Pump Shotgun Shell Holder (12 Gauge)
        CreateMagazine(
            basePath,
            "PumpShotgun_Shells",
            "Pump Shotgun 12GA Shell Holder",
            MagazineType.Shotgun,
            capacity: 6,
            reloadTime: 3.5f, // Помповая перезарядка по одному патрону
            tacticalReloadTime: 2.8f,
            weight: 0.6f
        );

        // Benelli M4 Magazine Tube (12 Gauge)
        CreateMagazine(
            basePath,
            "BenelliM4_Magazine",
            "Benelli M4 12GA Magazine Tube",
            MagazineType.Shotgun,
            capacity: 7,
            reloadTime: 3.0f, // Полуавтоматическая быстрее
            tacticalReloadTime: 2.5f,
            weight: 0.55f
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✓ Создано 8 пресетов MagazineData в {basePath}");
        EditorUtility.DisplayDialog("Готово", $"Создано 8 пресетов магазинов:\n- HK65 (Пистолет)\n- UMP (ПП)\n- AKM, AUG, CZ-907, M16 (Автоматы)\n- Pump Shotgun (Дробовик)\n- Benelli M4 (Автодробовик)", "OK");
    }

    private static void CreateMagazine(string basePath, string fileName, string displayName, 
        MagazineType type, int capacity, float reloadTime, float tacticalReloadTime, float weight)
    {
        MagazineData magazine = ScriptableObject.CreateInstance<MagazineData>();
        magazine.name = displayName;
        magazine.magazineName = displayName;
        magazine.ammoType = type;
        magazine.capacity = capacity;
        magazine.reloadTime = reloadTime;
        magazine.tacticalReloadTime = tacticalReloadTime;
        magazine.weight = weight;
        
        string fullPath = Path.Combine(basePath, fileName + ".asset");
        AssetDatabase.CreateAsset(magazine, fullPath);
        Debug.Log($"Создан магазин: {fullPath}");
    }
}
#endif
