using UnityEngine;

/// <summary>
/// Контроллер штурмовой винтовки
/// Поддерживает одиночный, автоматический и режим очереди
/// </summary>
public class WeaponPart_Assault : WeaponController
{
    [Header("Assault Rifle Settings")]
    public int pelletsPerShot = 1; // Для совместимости с системой дробовиков
    public float weaponSpread = 2f;
    
    private float _nextTimeToFire;
    private int _burstShotsRemaining;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Fire()
    {
        if (_currentAmmo <= 0) return;

        _currentAmmo--;
        TryShowAmmoHint();

        Vector3 direction = GetFireDirection(CalculateSpread());
        PerformHitScan(direction);

        EjectShell();

        if (_shotsFired >= GetAccurateShotsCount())
            _currentSpread = Mathf.Min(_currentSpread + _spreadIncreaseRate, _maxSpread);

        _shotsFired++;
        ApplyRecoil();

        if (_currentFireMode == FireMode.Burst)
            _burstShotsRemaining--;
    }

    protected override void ShouldFire()
    {
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
}
