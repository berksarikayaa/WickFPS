using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BossChaseAI : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float repathInterval = 0.15f;
    [SerializeField] private float stopDistance = 2.0f;

    private NavMeshAgent agent;
    private float timer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
    }

    void Update()
    {
        if (target == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = repathInterval;
            agent.SetDestination(target.position);
        }
    }

    void Start()
    {
        var melee = GetComponent<BossMeleeAttack>();
        if (melee != null && target != null)
            melee.SetTarget(target);
    }


    public void SetTarget(Transform t) => target = t;
}
