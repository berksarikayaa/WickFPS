using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyShooterAI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform target;           
    [SerializeField] private EnemyGunHitscan gun;

    [Header("Perception")]
    [SerializeField] private float viewDistance = 35f;
    [SerializeField] private float fovDegrees = 110f;
    [SerializeField] private LayerMask losMask = ~0;

    [Header("Aim")]
    [SerializeField] private float aimSmoothing = 10f;
    [SerializeField] private float aimFallbackHeight = 1.4f; 

    [Header("Burst Fire")]
    [SerializeField] private int burstMin = 2;
    [SerializeField] private int burstMax = 4;
    [SerializeField] private float burstGap = 0.07f;
    [SerializeField] private float burstCooldown = 0.6f;

    [Header("Movement / Distances")]
    [SerializeField] private float stopDistance = 10f;
    [SerializeField] private bool enableStrafe = true;
    [SerializeField] private float strafeRadius = 12f;
    [SerializeField] private float strafeOffset = 4f;
    [SerializeField] private Vector2 strafeChangeTime = new Vector2(0.8f, 1.6f);
    [SerializeField] private Vector2 pauseTime = new Vector2(0.2f, 0.6f);
    [SerializeField] private float repathInterval = 0.2f;

    [Header("Suppression / Hit Reaction")]
    [SerializeField] private float suppressMin = 0.25f;
    [SerializeField] private float suppressMax = 0.55f;
    [SerializeField] private float retreatDistanceOnHit = 2.5f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private float debugEverySeconds = 0.5f;
    [SerializeField] private bool forceLosMaskEverything = true;
    [SerializeField] private bool debugLosDetails = true;

    private NavMeshAgent agent;
    private EnemyHealthTwoStage health;
    private Transform selfRoot;

    private Collider targetCollider;

    private int strafeDir = 1;
    private float nextStrafeChange;
    private float pauseUntil;
    private float nextRepath;

    private float nextBurstTime;
    private int shotsLeftInBurst;
    private float nextShotInBurstTime;

    private float suppressedUntil;
    private float nextDebugTime;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        selfRoot = transform.root;

        if (transform != selfRoot)
        {
            if (debugLogs)
                Debug.LogWarning($"[AI] Disabled on child object '{name}'. Keep EnemyShooterAI only on ROOT '{selfRoot.name}'.");
            enabled = false;
            return;
        }

        if (gun == null)
            gun = GetComponentInChildren<EnemyGunHitscan>(true);

        if (target == null)
        {
            var ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null) target = ph.transform;
            else
            {
                var cc = FindFirstObjectByType<CharacterController>();
                if (cc != null) target = cc.transform;
            }
        }

        CacheTargetCollider();

        if (agent != null)
        {
            agent.stoppingDistance = stopDistance;
            agent.updateRotation = false;
        }

        health = GetComponent<EnemyHealthTwoStage>();
        if (health != null)
        {
            health.OnHit += HandleHit;
            health.OnDowned += HandleDowned;
            health.OnDied += HandleDied;
        }

        if (debugLogs)
            Debug.Log($"[AI] Awake ROOT '{name}' target={(target != null)} gun={(gun != null)} agent={(agent != null)}");
    }

    private void CacheTargetCollider()
    {
        targetCollider = null;
        if (target == null) return;

        targetCollider = target.GetComponent<Collider>();
        if (targetCollider == null)
            targetCollider = target.GetComponentInChildren<Collider>();
    }

    void Update()
    {
        if (target == null || gun == null) return;

        if (targetCollider == null) CacheTargetCollider();

        bool canMove = agent != null && agent.enabled && agent.isOnNavMesh;

        FaceTargetSmooth();

        Vector3 aimPoint = GetAimPoint();

        bool canSee = CanSeePoint(aimPoint, out string reason, out string losInfo);

        if (!canSee)
        {
            if (canMove)
            {
                agent.isStopped = false;

                Vector3 awayFromPlayer = (transform.position - target.position);
                awayFromPlayer.y = 0f;

                Vector3 chasePoint = target.position;
                if (awayFromPlayer.sqrMagnitude > 0.01f)
                    chasePoint = target.position + awayFromPlayer.normalized * agent.stoppingDistance;

                agent.SetDestination(chasePoint);
            }

            DebugTick(false, canMove, "NO_LOS", reason, losInfo);
            return;
        }

        if (Time.time < suppressedUntil)
        {
            if (canMove) agent.isStopped = true;
            DebugTick(true, canMove, "SUPPRESSED", "suppressed", "");
            return;
        }

        if (canMove) HandleMovement();

        if (gun.IsReloading)
        {
            DebugTick(true, canMove, "RELOADING", "gun reloading", "");
            return;
        }

        if (ShouldReload())
        {
            gun.TryReload();
            DebugTick(true, canMove, "TRY_RELOAD", "mag empty -> reload", "");
            return;
        }

        HandleBurstFire(aimPoint, canMove);
        DebugTick(true, canMove, "COMBAT", "ok", "");
    }

    private Vector3 GetAimPoint()
    {
        if (targetCollider != null)
        {
            return targetCollider.bounds.center;
        }

        // fallback
        return target.position + Vector3.up * aimFallbackHeight;
    }

    // ---------------- MOVEMENT ----------------

    private void HandleMovement()
    {
        if (Time.time < pauseUntil)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;

        if (enableStrafe && Time.time >= nextStrafeChange)
        {
            nextStrafeChange = Time.time + Random.Range(strafeChangeTime.x, strafeChangeTime.y);
            pauseUntil = Time.time + Random.Range(pauseTime.x, pauseTime.y);
            strafeDir = Random.value < 0.5f ? -1 : 1;
        }

        Vector3 toTarget = transform.position - target.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.01f) toTarget = transform.forward;

        Vector3 ringPos = target.position + toTarget.normalized * strafeRadius;
        Vector3 right = Vector3.Cross(Vector3.up, (ringPos - target.position).normalized);
        Vector3 strafePos = ringPos + right * (strafeOffset * strafeDir);

        if (Time.time >= nextRepath)
        {
            nextRepath = Time.time + repathInterval;
            agent.SetDestination(strafePos);
        }
    }

    // ---------------- FIRING ----------------

    private void HandleBurstFire(Vector3 aimPoint, bool canMove)
    {
        if (Time.time < nextBurstTime) return;

        if (shotsLeftInBurst <= 0)
        {
            shotsLeftInBurst = Random.Range(burstMin, burstMax + 1);
            nextShotInBurstTime = Time.time;
        }

        if (shotsLeftInBurst > 0 && Time.time >= nextShotInBurstTime)
        {
            if (debugLogs)
            {
                Debug.Log($"[AI FIRE] {name} FireAt | canMove={canMove} canFire={gun.CanFireNow()} reloading={gun.IsReloading} hasAmmo={gun.HasAmmo} shotsLeft={shotsLeftInBurst}");
            }

            gun.FireAt(aimPoint);

            shotsLeftInBurst--;
            nextShotInBurstTime = Time.time + burstGap;

            if (shotsLeftInBurst <= 0)
                nextBurstTime = Time.time + burstCooldown;
        }
    }

    private bool ShouldReload()
    {
        return !gun.IsReloading && !gun.CanFireNow() && gun.HasAmmo;
    }

    // ---------------- LOS ----------------

    private bool CanSeePoint(Vector3 point, out string reason, out string losInfo)
    {
        reason = "";
        losInfo = "";

        Vector3 eye = transform.position + Vector3.up * 1.6f;
        Vector3 toPoint = point - eye;

        float dist = toPoint.magnitude;
        if (dist > viewDistance)
        {
            reason = $"too far dist={dist:F1}>{viewDistance:F1}";
            return false;
        }

        float angle = Vector3.Angle(transform.forward, toPoint);
        if (angle > fovDegrees * 0.5f)
        {
            reason = $"out of fov angle={angle:F1}>{(fovDegrees * 0.5f):F1}";
            return false;
        }

        Vector3 dir = toPoint / Mathf.Max(dist, 0.0001f);
        eye += dir * 0.05f;

        LayerMask maskToUse = forceLosMaskEverything ? ~0 : losMask;

        var hits = Physics.RaycastAll(eye, dir, viewDistance, maskToUse, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            reason = "raycast no hits";
            return false;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        if (debugLosDetails)
        {
            int max = Mathf.Min(4, hits.Length);
            string s = $"hits={hits.Length} ";
            for (int i = 0; i < max; i++)
            {
                var c = hits[i].collider;
                if (c == null) continue;
                s += $"[{i}:{c.name}/L{LayerMask.LayerToName(c.gameObject.layer)}/d{hits[i].distance:F2}] ";
            }
            losInfo = s;
        }

        int selfHits = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i].collider;
            if (c == null) continue;

            if (c.transform.IsChildOf(selfRoot))
            {
                selfHits++;
                continue;
            }

            bool isPlayer = c.GetComponentInParent<PlayerHealth>() != null || c.GetComponentInParent<FPSPlayerController>() != null;
            reason = isPlayer ? "LOS ok" : $"blocked by {c.name} layer={LayerMask.LayerToName(c.gameObject.layer)}";
            return isPlayer;
        }

        reason = $"only self hits (count={selfHits})";
        return false;
    }

    private void FaceTargetSmooth()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * aimSmoothing);
    }

    // ---------------- HIT REACTION ----------------

    private void HandleHit(HitInfo hit)
    {
        suppressedUntil = Time.time + Random.Range(suppressMin, suppressMax);

        shotsLeftInBurst = 0;
        nextBurstTime = Time.time + 0.15f;

        bool canMove = agent != null && agent.enabled && agent.isOnNavMesh;
        if (!canMove || target == null) return;

        Vector3 away = transform.position - target.position;
        away.y = 0f;

        if (away.sqrMagnitude > 0.01f)
        {
            Vector3 retreat = transform.position + away.normalized * retreatDistanceOnHit;
            agent.SetDestination(retreat);
        }
    }

    private void HandleDowned()
    {
        suppressedUntil = Time.time + 999f;
        shotsLeftInBurst = 0;

        bool canMove = agent != null && agent.enabled && agent.isOnNavMesh;
        if (canMove) agent.isStopped = true;
    }

    private void HandleDied()
    {
        if (health != null)
        {
            health.OnHit -= HandleHit;
            health.OnDowned -= HandleDowned;
            health.OnDied -= HandleDied;
        }
    }

    // ---------------- DEBUG ----------------

    private void DebugTick(bool canSee, bool canMove, string state, string reason, string losInfo)
    {
        if (!debugLogs) return;
        if (Time.time < nextDebugTime) return;

        nextDebugTime = Time.time + Mathf.Max(0.1f, debugEverySeconds);

        Debug.Log($"[AI STATE] {name} state={state} canSee={canSee} canMove={canMove} " +
                  $"suppressed={(Time.time < suppressedUntil)} reloading={gun.IsReloading} " +
                  $"canFire={gun.CanFireNow()} hasAmmo={gun.HasAmmo} reason={reason} {losInfo}");
    }
}
