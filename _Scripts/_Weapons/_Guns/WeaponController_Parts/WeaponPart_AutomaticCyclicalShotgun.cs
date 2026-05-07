using UnityEngine;
using System.Collections;
using LightSide.Core;

/// <summary>
/// Контроллер автоматического дробовика с покадровой перезарядкой
/// Поддерживает прерывание перезарядки при нажатии огня
/// </summary>
public class WeaponPart_AutomaticCyclicalShotgun : WeaponController
{
    [Header("Shotgun Settings")]
    public bool canInterruptReload = true;
    public float shellReloadTime = 0.8f;
    
    [Header("Pellet Settings")]
    public int pelletsPerShot = 8;
    public float shotgunSpread = 15f;
    
    private float _nextTimeToFire;
    private int _burstShotsRemaining;
    private Coroutine _shellReloadCoroutine;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Fire()
    {
        if (_currentAmmo <= 0) return;

        _currentAmmo--;
        TryShowAmmoHint();

        // Выпускаем все дробины залпом
        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 direction = GetFireDirection(shotgunSpread);
            PerformHitScan(direction);
        }

        EjectShell();
        _shotsFired++;
        ApplyRecoil();

        if (_currentFireMode == FireMode.Burst)
            _burstShotsRemaining--;
    }

    protected override void ShouldFire()
    {
        // Прерываем перезарядку при нажатии огня
        if (_isReloading && canInterruptReload && _playerInput?.firePressed == true)
        {
            InterruptReload();
        }

        bool isFiring = false;
        float currentInterval = _fireInterval;

        switch (_currentFireMode)
        {
            case FireMode.Single:
                isFiring = _playerInput?.firePressed == true && Time.time >= _nextTimeToFire;
                break;
                
            case FireMode.Burst:
                if (_burstShotsRemaining <= 0 && _playerInput?.firePressed == true)
                {
                    _burstShotsRemaining = _burstLength;
                    isFiring = true;
                    currentInterval = _burstInterval;
                }
                else if (Time.time >= _nextTimeToFire && _burstShotsRemaining > 0)
                {
                    isFiring = true;
                    currentInterval = _burstInterval;
                }
                break;
                
            case FireMode.Auto:
                isFiring = _playerInput?.fireHeld == true && Time.time >= _nextTimeToFire;
                break;
        }

        if (isFiring)
        {
            Fire();
            _nextTimeToFire = Time.time + currentInterval;
            _lastFireTime = Time.time;
        }
    }

    protected override void StartReload()
    {
        _isReloading = true;
        _currentSpread = 0f;
        _shotsFired = 0;

        string reloadAnim = (_currentAmmo == 0) ? "LoadShell" : "Reload";
        _animator?.SetTrigger(reloadAnim);

        _shellReloadCoroutine = StartCoroutine(ReloadShellRoutine());
        
        EventBus.InvokeReloadStarted(weaponData);
    }

    private IEnumerator ReloadShellRoutine()
    {
        while (_currentAmmo < _maxAmmo)
        {
            yield return new WaitForSeconds(shellReloadTime);

            _currentAmmo++;
            
            EventBus.InvokeAmmoChanged(weaponData, _currentAmmo, _maxAmmo);
        }

        FinishReload();
    }

    private void InterruptReload()
    {
        if (_shellReloadCoroutine != null)
        {
            StopCoroutine(_shellReloadCoroutine);
            _shellReloadCoroutine = null;
        }

        _isReloading = false;
        TryShowAmmoHint();
    }

    protected override void FinishReload()
    {
        _isReloading = false;
        _shellReloadCoroutine = null;
        
        TryShowAmmoHint();
        EventBus.InvokeReloadCompleted(weaponData);
    }

    protected override void OnDisable()
    {
        if (_shellReloadCoroutine != null)
        {
            StopCoroutine(_shellReloadCoroutine);
            _shellReloadCoroutine = null;
        }

        _isReloading = false;

        base.OnDisable();
    }
}
