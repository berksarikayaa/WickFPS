using UnityEngine;

public class EnemyGunHitscan : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform muzzle;

    [Header("Damage")]
    [SerializeField] private int damage = 12;

    [Header("Range & Accuracy")]
    [SerializeField] private float range = 60f;
    [SerializeField] private float spreadDegrees = 1.5f;

    [Header("Fire")]
    [SerializeField] private float fireRate = 0.18f;
    private float nextFireTime;

    [Header("Ammo (Magazine + Reserve)")]
    [SerializeField] private int magSize = 15;
    [SerializeField] private int magAmmo;
    [SerializeField] private int reserveAmmo = 45;
    [SerializeField] private float reloadTime = 1.4f;
    private bool isReloading;

    [Header("VFX (optional)")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private LineRenderer tracerPrefab;
    [SerializeField] private float tracerDuration = 0.05f;
    [SerializeField] private ParticleSystem hitImpactPrefab;

    [Header("Hit Mask")]
    [SerializeField] private LayerMask hitMask = ~0;


    void Awake()
    {
        magAmmo = magSize;
        if (muzzle == null) Debug.LogError("EnemyGunHitscan: muzzle atanmadý.");

        Debug.Log($"[EnemyGunHitscan] Awake: {name}"); 
    }

    void OnEnable() => Debug.Log($"[EnemyGunHitscan] OnEnable: {name}");


    public bool IsReloading => isReloading;
    public bool HasAmmo => magAmmo > 0 || reserveAmmo > 0;

    public bool CanFireNow()
    {
        if (isReloading) return false;
        if (Time.time < nextFireTime) return false;
        if (magAmmo <= 0) return false;
        return true;
    }

    public void TryReload()
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

    public void FireAt(Vector3 targetPoint)
    {
        Debug.Log($"[Gun] FireAt called {name}");

        if (!CanFireNow()) return;

        nextFireTime = Time.time + fireRate;
        magAmmo--;

        if (muzzleFlash != null) muzzleFlash.Play();

        Vector3 start = muzzle != null ? muzzle.position : transform.position;
        Vector3 dir = (targetPoint - start).normalized;

        dir = ApplySpread(dir, spreadDegrees);

        start += dir * 0.05f;

        Ray ray = new Ray(start, dir);
        Vector3 end = start + dir * range;

        if (TryRaycastIgnoreSelf(ray, out RaycastHit hit, range))
        {
            end = hit.point;

            if (hitImpactPrefab != null)
                Instantiate(hitImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));

            var playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);
        }

        SpawnTracer(start, end);
    }

    private bool TryRaycastIgnoreSelf(Ray ray, out RaycastHit hit, float maxDistance)
    {
        var hits = Physics.RaycastAll(ray, maxDistance, hitMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            hit = default;
            return false;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider != null && hits[i].collider.transform.IsChildOf(transform))
                continue;

            hit = hits[i];
            return true;
        }

        hit = default;
        return false;
    }

    private Vector3 ApplySpread(Vector3 dir, float degrees)
    {
        if (degrees <= 0.001f) return dir;

        float yaw = Random.Range(-degrees, degrees);
        float pitch = Random.Range(-degrees, degrees);

        Quaternion q = Quaternion.Euler(pitch, yaw, 0f);
        return (q * dir).normalized;
    }

    private void SpawnTracer(Vector3 start, Vector3 end)
    {
        if (tracerPrefab == null) return;

        LineRenderer tracer = Instantiate(tracerPrefab);
        tracer.SetPosition(0, start);
        tracer.SetPosition(1, end);

        Destroy(tracer.gameObject, tracerDuration);
    }

    public int MagAmmo => magAmmo;
    public int MagSize => magSize;
    public int ReserveAmmo => reserveAmmo;
}
