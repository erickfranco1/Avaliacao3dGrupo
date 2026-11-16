using UnityEngine;

// Cria obstaculos aleatorios nas 3 faixas, cada vez mais rapido conforme a velocidade sobe.
public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public float laneDistance = 2.5f;       // igual ao do PlayerRunner
    public float spawnZ = 40f;
    public float minInterval = 0.9f;
    public float maxInterval = 1.8f;

    float timer = 1f;

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || !gm.IsPlaying || obstaclePrefabs.Length == 0) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        int lane = Random.Range(-1, 2);     // -1, 0 ou 1
        var prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        Instantiate(prefab, new Vector3(lane * laneDistance, 0f, spawnZ), Quaternion.identity);

        // quanto mais rapido o mundo, menor o intervalo entre obstaculos
        float factor = gm.startSpeed / gm.Speed;
        timer = Random.Range(minInterval, maxInterval) * factor;
    }
}