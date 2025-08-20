using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public Monster monsterToSpawn;
    public GameObject monsterSpawnEffect;

    [Header("Spawn Shape")]
    public float spawnRadius = 12f;
    public float minSpawnRadius = 2f;
    public Transform[] spawnPoints;
    public LayerMask groundMask = ~0;

    [Header("Wave Tuning")]
    public int maxAlive = 25;
    public float nearPlayerRadius = 15f;
    public Vector2Int groupSizeNear = new Vector2Int(3, 6);
    public Vector2Int groupSizeFar = new Vector2Int(1, 3);
    public float waveCooldownNear = 6f;
    public float waveCooldownFar = 12f;
    public float intraSpawnDelay = 0.25f;

    [Header("Ambient Far Spawns")]
    public float farWaveRollInterval = 3f;
    [Range(0f, 1f)] public float farWaveChance = 0.35f;

    [Header("Difficulty Scaling")]
    public int bonusPerMinute = 1;
    public int capBonus = 15;

    [Header("NavMesh Sampling")]
    public float navSampleDistance = 4f;
    public int navSampleTries = 8;

    private float _nextCheckAt;
    private float _nextFarRollAt;
    private bool _spawning;
    private readonly List<GameObject> _tracked = new List<GameObject>();
    private Player _player;

    void Awake()
    {
        _player = FindObjectOfType<Player>();
        _nextCheckAt = Time.time + 1f;
        _nextFarRollAt = Time.time + farWaveRollInterval;
    }

    void Update()
    {
        CleanupTracked();

        if (monsterToSpawn == null) return;
        if (_spawning) return;
        if (CountAlive() >= maxAlive) return;

        bool near = IsPlayerNear();

        if (Time.time >= _nextCheckAt && near)
        {
            int size = Mathf.Clamp(RandomGroupSize(groupSizeNear) + DifficultyBonus(), 1, 20);
            StartCoroutine(SpawnWave(size));
            _nextCheckAt = Time.time + waveCooldownNear;
            return;
        }

        if (Time.time >= _nextFarRollAt && !near)
        {
            _nextFarRollAt = Time.time + farWaveRollInterval;
            if (Random.value < farWaveChance)
            {
                int size = RandomGroupSize(groupSizeFar) + Mathf.Max(0, DifficultyBonus() - 1);
                StartCoroutine(SpawnWave(size));
            }
        }

        if (Time.time >= _nextCheckAt && !near)
            _nextCheckAt = Time.time + waveCooldownFar;
    }

    IEnumerator SpawnWave(int count)
    {
        _spawning = true;

        int spawned = 0;
        while (spawned < count && CountAlive() < maxAlive)
        {
            int burst = Mathf.Min(Random.Range(2, 5), count - spawned, maxAlive - CountAlive());
            for (int i = 0; i < burst; i++)
            {
                if (!TryGetNavmeshSpawnPosition(out Vector3 pos)) continue;

                var go = Instantiate(monsterToSpawn.gameObject, pos, Quaternion.identity);
                EnsureOnNavmesh(go);
                _tracked.Add(go);

                if (monsterSpawnEffect != null)
                    Instantiate(monsterSpawnEffect, pos, Quaternion.identity);
            }
            spawned += burst;

            yield return new WaitForSeconds(intraSpawnDelay * Random.Range(0.6f, 1.4f));
        }

        _spawning = false;
    }

    bool TryGetNavmeshSpawnPosition(out Vector3 result)
    {
        for (int t = 0; t < Mathf.Max(1, navSampleTries); t++)
        {
            Vector3 candidate;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                var p = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Vector2 jitter = Random.insideUnitCircle * Random.Range(minSpawnRadius, spawnRadius);
                candidate = p.position + new Vector3(jitter.x, 0f, jitter.y);
            }
            else
            {
                float r = Random.Range(minSpawnRadius, spawnRadius);
                Vector2 ring = Random.insideUnitCircle.normalized * r;
                candidate = transform.position + new Vector3(ring.x, 0f, ring.y);
            }

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, navSampleDistance, NavMesh.AllAreas))
            {
                Vector3 p = hit.position;
                if (Physics.Raycast(p + Vector3.up * 10f, Vector3.down, out RaycastHit h, 50f, groundMask))
                    p = h.point;

                result = p;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }

    void EnsureOnNavmesh(GameObject go)
    {
        var agent = go.GetComponent<NavMeshAgent>();
        if (agent == null) return;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(go.transform.position, out NavMeshHit hit, navSampleDistance * 2f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }
    }

    bool IsPlayerNear()
    {
        if (_player == null) _player = FindObjectOfType<Player>();
        if (_player == null) return false;
        return Vector3.Distance(_player.transform.position, transform.position) <= nearPlayerRadius;
    }

    int CountAlive()
    {
        for (int i = _tracked.Count - 1; i >= 0; i--)
            if (_tracked[i] == null) _tracked.RemoveAt(i);
        return _tracked.Count;
    }

    void CleanupTracked()
    {
        for (int i = _tracked.Count - 1; i >= 0; i--)
            if (_tracked[i] == null) _tracked.RemoveAt(i);
    }

    int RandomGroupSize(Vector2Int range)
    {
        if (range.x > range.y) (range.x, range.y) = (range.y, range.x);
        return Random.Range(range.x, range.y + 1);
    }

    int DifficultyBonus()
    {
        float minutes = (Time.timeSinceLevelLoad / 60f);
        int bonus = Mathf.Min(Mathf.FloorToInt(minutes) * bonusPerMinute, capBonus);
        return Mathf.Max(0, bonus);
    }
}
