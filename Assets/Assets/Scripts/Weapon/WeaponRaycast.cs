using UnityEngine;

public class WeaponRaycast : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Camera cam;
    [SerializeField] private float range = 80f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Ammo (Magazine + Reserve)")]
    [SerializeField] private int magSize = 18;
    [SerializeField] private int magAmmo;
    [SerializeField] private int reserveAmmo = 54;
    [SerializeField] private int reserveMax = 180;

    [Header("Reload")]
    [SerializeField] private float reloadTime = 1.2f;
    private bool isReloading;

    [Header("Fire")]
    [SerializeField] private float fireRate = 0.12f;
    private float nextFireTime;

    [Header("VFX")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private LineRenderer tracerPrefab;
    [SerializeField] private float tracerDuration = 0.05f;
    [SerializeField] private ParticleSystem hitImpactPrefab;

    [Header("Recoil")]
    [SerializeField] private WeaponRecoilSway recoil;

    void Awake()
    {
        if (cam == null) Debug.LogError("WeaponRaycast: Camera atanmadý.");

        magAmmo = magSize;

        if (recoil == null) recoil = GetComponentInChildren<WeaponRecoilSway>();
    }

    void Update()
    {
        if (isReloading) return;

        if (Input.GetMouseButton(0))
            TryFire();

        if (Input.GetKeyDown(KeyCode.R))
            TryReload();
    }

    private void TryFire()
    {
        if (Time.time < nextFireTime) return;
        if (magAmmo <= 0)
        {
            //TryReload();
            return;
        }

        nextFireTime = Time.time + fireRate;
        magAmmo--;

        if (recoil != null) recoil.AddRecoil();

        if (muzzleFlash != null)
            muzzleFlash.Play();

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Vector3 hitPoint = cam.transform.position + cam.transform.forward * range;

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            hitPoint = hit.point;

            if (hitImpactPrefab != null)
            {
                Instantiate(hitImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }

            bool isHeadshot = hit.collider.CompareTag("Head");

            var damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeHit(new HitInfo
                {
                    point = hit.point,
                    normal = hit.normal,
                    isHeadshot = isHeadshot
                });
            }
        }

        SpawnTracer(cam.transform.position, hitPoint);
    }

    private void TryReload()
    {
        if (isReloading) return;
        if (magAmmo >= magSize) return;       
        if (reserveAmmo <= 0) return;         

        StartCoroutine(ReloadRoutine());
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        int need = magSize - magAmmo;
        int take = Mathf.Min(need, reserveAmmo);

        magAmmo += take;
        reserveAmmo -= take;

        isReloading = false;
    }

    private void SpawnTracer(Vector3 start, Vector3 end)
    {
        if (tracerPrefab == null) return;

        LineRenderer tracer = Instantiate(tracerPrefab);
        tracer.SetPosition(0, start);
        tracer.SetPosition(1, end);

        Destroy(tracer.gameObject, tracerDuration);
    }

    // --- Pickup API ---
    public void AddReserveAmmo(int amount)
    {
        reserveAmmo = Mathf.Clamp(reserveAmmo + amount, 0, reserveMax);
    }

    public void AddMagazineAmmo(int amount)
    {
        magAmmo = Mathf.Clamp(magAmmo + amount, 0, magSize);
    }

    // --- UI ---
    public int MagAmmo => magAmmo;
    public int MagSize => magSize;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => isReloading;
}

public struct HitInfo
{
    public Vector3 point;
    public Vector3 normal;
    public bool isHeadshot;
}

public interface IDamageable
{
    void TakeHit(HitInfo hit);
}
