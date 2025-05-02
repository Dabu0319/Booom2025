using UnityEngine;

public class Dabu10_BirdSpawnManager : MonoBehaviour
{
    public GameObject birdPrefab;
    public float spawnInterval = 2f;
    public float radius = 5f;
    public Vector2 center = Vector2.zero;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnBird();
        }
    }

    void SpawnBird()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 spawnPos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

        Vector2 dirToCenter = (center - spawnPos).normalized;
        float angleToCenter = Mathf.Atan2(dirToCenter.y, dirToCenter.x) * Mathf.Rad2Deg;

        float modelOffset = 45f; // 因为鸟的默认朝向是 (1,1)
        Quaternion rot = Quaternion.Euler(0, 0, angleToCenter - modelOffset);

        GameObject bird = Instantiate(birdPrefab, spawnPos, rot);
        Destroy(bird, 10f); // Optional: Destroy the bird after 10 seconds
        
    }
}
