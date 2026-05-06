/// <summary>
/// Контроллер магазинного дробовика с автоматической стрельбой
/// Перезаряжается полным магазином за один раз
/// </summary>
public class WeaponPart_AutomaticMagazineShotgun : WeaponController
{
    [Header("Pellet Settings")]
    public int pelletsPerShot = 8;
    public float shotgunSpread = 15f;
    
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
