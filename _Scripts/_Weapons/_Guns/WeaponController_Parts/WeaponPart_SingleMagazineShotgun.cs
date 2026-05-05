using UnityEngine;

public class WeaponPart_SingleMagazineShotgun : WeaponController
{
    public bool canInterruptReload = true;
    public float shellReloadTime = 0.8f;
    private float nextTimeToFire = 0f;

    protected new void Start() => base.Start();

    protected override void Fire()
    {
        if (currentAmmo <= 0) return;

        currentAmmo--;
        TryShowAmmoHint();

        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 direction = GetFireDirection(shotgunSpread);
            PerformHitScan(direction);
        }

        EjectShell();
        shotsFired++;
        ApplyRecoil();

        nextTimeToFire = Time.time + fireInterval;
    }

    protected override void ShouldFire()
    {
        bool isFiring = playerInput.firePressed && Time.time >= nextTimeToFire;

        if (isFiring)
        {
            Fire();
            nextTimeToFire = Time.time + fireInterval;
            lastFireTime = Time.time;
        }
    }
}
