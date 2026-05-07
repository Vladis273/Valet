using UnityEngine;

/// <summary>
/// Контроллер однозарядного магазинного дробовика
/// Перезаряжается полным магазином за один раз
/// </summary>
public class WeaponPart_SingleMagazineShotgun : WeaponController
{
    [Header("Pellet Settings")]
    public int pelletsPerShot = 8;
    public float shotgunSpread = 15f;
    
    private float _nextTimeToFire;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Fire()
    {
        if (!ConsumeAmmo()) return;

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
        bool isFiring = _playerInput?.firePressed == true && Time.time >= _nextTimeToFire;

        if (isFiring)
        {
            Fire();
            _nextTimeToFire = Time.time + _fireInterval;
            _lastFireTime = Time.time;
        }
    }
}
