using UnityEngine;
using System.Collections;

/// <summary>
/// Контроллер однозарядного дробовика с покадровой перезарядкой
/// Поддерживает прерывание перезарядки при нажатии огня
/// </summary>
public class WeaponPart_SingleCyclicalShotgun : WeaponController
{
    [Header("Shotgun Reload Settings")]
    public bool canInterruptReload = true;
    public float shellReloadTime = 0.8f;
    
    [Header("Pellet Settings")]
    public int pelletsPerShot = 8;
    public float shotgunSpread = 15f;
    
    private float _nextTimeToFire;
    private Coroutine _reloadCoroutine;

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

        _nextTimeToFire = Time.time + _fireInterval;
    }

    protected override void ShouldFire()
    {
        // Прерываем перезарядку при нажатии огня
        if (_isReloading && canInterruptReload && _playerInput?.firePressed == true)
        {
            InterruptReload();
        }

        bool isFiring = _playerInput?.firePressed == true && Time.time >= _nextTimeToFire;

        if (isFiring)
        {
            Fire();
            _nextTimeToFire = Time.time + _fireInterval;
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

        _reloadCoroutine = StartCoroutine(ReloadShellRoutine());
        
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
        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }

        _isReloading = false;
        TryShowAmmoHint();
    }

    protected override void FinishReload()
    {
        _isReloading = false;
        _reloadCoroutine = null;
        
        TryShowAmmoHint();
        EventBus.InvokeReloadCompleted(weaponData);
    }

    protected override void OnDisable()
    {
        if (_reloadCoroutine != null)
        {
            StopCoroutine(_reloadCoroutine);
            _reloadCoroutine = null;
        }

        _isReloading = false;

        base.OnDisable();
    }
}
