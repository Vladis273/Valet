using UnityEngine;

namespace LightSide.Core
{
    /// <summary>
    /// Легковесная система событий для слабой связанности компонентов
    /// </summary>
    public static class EventBus
    {
        // События оружия
        public static event System.Action<WeaponData, int, int> OnAmmoChanged;
        public static event System.Action<WeaponData, FireMode> OnFireModeChanged;
        public static event System.Action<WeaponData> OnWeaponFired;
        public static event System.Action<WeaponData> OnReloadStarted;
        public static event System.Action<WeaponData> OnReloadCompleted;
        
        // События игрока
        public static event System.Action<float> OnPlayerHealthChanged;
        public static event System.Action OnPlayerDied;
        public static event System.Action<PlayerPose> OnPlayerPoseChanged;
        
        // События гранат
        public static event System.Action<GrenadeType, Vector3> OnGrenadeExploded;
        
        // Методы для вызова событий
        public static void InvokeAmmoChanged(WeaponData weapon, int current, int max) => 
            OnAmmoChanged?.Invoke(weapon, current, max);
            
        public static void InvokeFireModeChanged(WeaponData weapon, FireMode mode) => 
            OnFireModeChanged?.Invoke(weapon, mode);
            
        public static void InvokeWeaponFired(WeaponData weapon) => 
            OnWeaponFired?.Invoke(weapon);
            
        public static void InvokeReloadStarted(WeaponData weapon) => 
            OnReloadStarted?.Invoke(weapon);
            
        public static void InvokeReloadCompleted(WeaponData weapon) => 
            OnReloadCompleted?.Invoke(weapon);
            
        public static void InvokePlayerHealthChanged(float health) => 
            OnPlayerHealthChanged?.Invoke(health);
            
        public static void InvokePlayerDied() => 
            OnPlayerDied?.Invoke();
            
        public static void InvokePlayerPoseChanged(PlayerPose pose) => 
            OnPlayerPoseChanged?.Invoke(pose);
            
        public static void InvokeGrenadeExploded(GrenadeType type, Vector3 position) => 
            OnGrenadeExploded?.Invoke(type, position);
    }
}
