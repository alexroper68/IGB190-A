using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public float timeBetweenSpawns = 2.0f;
    public float spawnRadius = 10.0f;
    public Monster monsterToSpawn;
    public GameObject monsterSpawnEffect;

    private float nextSpawnAt = 0f;

    void Update()
    {
        if (monsterToSpawn != null && Time.time > nextSpawnAt)
        {
            // pick a random point within spawnRadius on the same Y level
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPosition.y = transform.position.y;

            nextSpawnAt = Time.time + timeBetweenSpawns;

            // spawn the monster prefab
            Instantiate(monsterToSpawn.gameObject, spawnPosition, transform.rotation);

            // spawn the optional effect
            if (monsterSpawnEffect != null)
                Instantiate(monsterSpawnEffect, spawnPosition, Quaternion.identity);
        }
    }
}
