using UnityEngine;

public class BossMeleeAttack : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2.2f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private int damage = 20;

    private float nextAttackTime;

    void Update()
    {
        if (target == null) return;
        if (Time.time < nextAttackTime) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > attackRange) return;

        var hp = target.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    public void SetTarget(Transform t) => target = t;
}
