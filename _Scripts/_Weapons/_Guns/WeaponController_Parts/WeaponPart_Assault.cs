using UnityEngine;

public class WeaponPart_Assault : WeaponController
{
    private float nextTimeToFire = 0;
    private int burstShotsRemaining = 0;

    protected new void Start()
    {
        base.Awake();
        base.Start();
    }

    protected override void Fire()
    {
        if (currentAmmo <= 0) return;

        currentAmmo--;
        TryShowAmmoHint();

        Vector3 direction = GetFireDirection(CalculateSpread());
        PerformHitScan(direction);

        EjectShell();

        if (shotsFired >= GetAccurateShotsCount())
            currentSpread = Mathf.Min(currentSpread + spreadIncreaseRate, maxSpread);

        shotsFired++;
        ApplyRecoil();

        if (currentFireMode == FireMode.Burst)
            burstShotsRemaining--;
    }

    protected override void ShouldFire()
    {
        bool isFiring = false;
        float currectInterval = fireInterval;

        switch (currentFireMode)
        {
            case FireMode.Single:
                isFiring = playerInput.firePressed && Time.time >= nextTimeToFire;
                break;
            case FireMode.Burst:
                if (burstShotsRemaining <= 0 && playerInput.firePressed)
                {
                    burstShotsRemaining = burstLength;
                    isFiring = true;
                    currectInterval = burstInterval;
                }
                else if (Time.time >= nextTimeToFire && burstShotsRemaining > 0)
                {
                    isFiring = true;
                    currectInterval = burstInterval;
                }
                break;
            case FireMode.Auto:
                isFiring = playerInput.fireHeld && Time.time >= nextTimeToFire;
                break;
        }

        if (isFiring)
        {
            Fire();
            nextTimeToFire = Time.time + currectInterval;
            lastFireTime = Time.time;
        }
    }
}
