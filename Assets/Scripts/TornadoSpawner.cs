using UnityEngine;

public class TornadoSpawner : MonoBehaviour
{
    public GameObject tornadoPrefab;
    public Vector2 spawnRangeX = new Vector2(-10f, 10f);
    public Vector2 spawnRangeY = new Vector2(-7f, 7f);

    [Header("Progress Settings")]
    public PlayerController playerScript;
    public float startThreshold = 0.5f;

    [Header("Difficulty Curves")]
    public float maxInterval = 5f;
    public float minInterval = 1f;

    private float timer;

    void Update()
    {
        if (playerScript == null) return;

        float progress = GetPlayerProgress();

        if (progress >= startThreshold)
        {
            float difficultyT = Mathf.InverseLerp(startThreshold, 1f, progress);

            float currentInterval = Mathf.Lerp(maxInterval, minInterval, difficultyT);

            timer += Time.deltaTime;

            if (timer >= currentInterval)
            {
                SpawnTornado();
                timer = 0;
            }
        }
    }

    float GetPlayerProgress()
    {
        return (float)playerScript.GetCurrentScore() / playerScript.endingPoint;
    }

    void SpawnTornado()
    {
        if (tornadoPrefab == null) return;
        float randomX = Random.Range(spawnRangeX.x, spawnRangeX.y);
        float randomY = Random.Range(spawnRangeY.x, spawnRangeY.y);
        Vector3 spawnPos = new Vector3(randomX, randomY, 0);
        Instantiate(tornadoPrefab, spawnPos, Quaternion.identity);
    }
}