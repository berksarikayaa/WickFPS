using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float navMeshSampleRadius = 4f;

    [ContextMenu("Spawn One")]
    public void SpawnOne()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 desired = sp.position;

        if (NavMesh.SamplePosition(desired, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            desired = hit.position;
        }

        Instantiate(enemyPrefab, desired, sp.rotation);
    }
}
