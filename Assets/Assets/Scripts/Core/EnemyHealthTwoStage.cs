using System;
using UnityEngine;

public class EnemyHealthTwoStage : MonoBehaviour, IDamageable
{
    public enum State { Alive, Downed, Dead }

    [SerializeField] private State state = State.Alive;

    [Header("Downed Settings")]
    [SerializeField] private float downedDuration = 6f;
    private float downedTimer;

    [Header("Optional visuals")]
    [SerializeField] private Transform bodyRoot;
    [SerializeField] private float downedTilt = 70f;

    [Header("Ammo Drop On Death")]
    [SerializeField] private AmmoPickup ammoPickupPrefab; 
    [SerializeField, Range(0f, 1f)] private float dropChance = 0.65f;
    [SerializeField] private Vector2Int reserveDropRange = new Vector2Int(6, 18);
    [SerializeField] private Vector3 dropOffset = new Vector3(0f, 0.1f, 0f);

    public event Action<HitInfo> OnHit;
    public event Action OnDowned;
    public event Action OnDied;

    void Update()
    {
        if (state == State.Downed)
        {
            downedTimer -= Time.deltaTime;
            if (downedTimer <= 0f)
            {
                ReviveFromDowned();
            }
        }
    }

    public void TakeHit(HitInfo hit)
    {
        if (state == State.Dead) return;

        OnHit?.Invoke(hit);

        if (hit.isHeadshot)
        {
            Die();
            return;
        }

        if (state == State.Alive)
        {
            GoDowned();
        }
        else if (state == State.Downed)
        {
            Die();
        }
    }

    private void GoDowned()
    {
        state = State.Downed;
        downedTimer = downedDuration;

        if (bodyRoot != null)
            bodyRoot.localRotation = Quaternion.Euler(downedTilt, 0f, 0f);

        OnDowned?.Invoke();
    }

    private void ReviveFromDowned()
    {
        state = State.Alive;

        if (bodyRoot != null)
            bodyRoot.localRotation = Quaternion.identity;
    }

    private void Die()
    {
        state = State.Dead;

        TrySpawnAmmoDrop();

        OnDied?.Invoke();

        gameObject.SetActive(false);
    }

    private void TrySpawnAmmoDrop()
    {
        if (ammoPickupPrefab == null) return;
        if (UnityEngine.Random.value > dropChance) return;

        Vector3 pos = transform.position + dropOffset;
        AmmoPickup drop = Instantiate(ammoPickupPrefab, pos, Quaternion.identity);

        int reserve = UnityEngine.Random.Range(reserveDropRange.x, reserveDropRange.y + 1);
        drop.Configure(reserve);
    }
}
