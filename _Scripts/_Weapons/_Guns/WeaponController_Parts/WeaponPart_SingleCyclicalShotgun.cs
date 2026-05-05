using UnityEngine;
using System.Collections;

public class WeaponPart_SingleCyclicalShotgun : WeaponController
{
    [Header("Shotgun Reload Settings")]
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
        if (isReloading && canInterruptReload && playerInput.firePressed)
            InterruptReload();

        bool isFiring = playerInput.firePressed && Time.time >= nextTimeToFire;

        if (isFiring)
        {
            Fire();
            nextTimeToFire = Time.time + fireInterval;
            lastFireTime = Time.time;
        }
    }

    protected override void StartReload()
    {
        isReloading = true;
        currentSpread = 0f;
        shotsFired = 0;

        string reloadAnim = (currentAmmo == 0) ? "LoadShell" : "Reload";
        animator.SetTrigger(reloadAnim);

        reloadCoroutine = StartCoroutine(ReloadShellRoutine());
    }

    private IEnumerator ReloadShellRoutine()
    {
        while (currentAmmo < maxAmmo)
        {
            yield return new WaitForSeconds(shellReloadTime);

            currentAmmo++;

            Debug.Log($"1 loaded, Ammo: {currentAmmo}");
            TryShowAmmoHint();
        }

        FinishReload();
    }

    private void InterruptReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        isReloading = false;
        Debug.Log("Reload interrupted");
        TryShowAmmoHint();
    }

    protected override void FinishReload()
    {
        isReloading = false;
        isTacticalReload = false;
        reloadCoroutine = null;

        TryShowAmmoHint();
    }

    protected override void OnDisable()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        isReloading = false;

        base.OnDisable();
    }
}
