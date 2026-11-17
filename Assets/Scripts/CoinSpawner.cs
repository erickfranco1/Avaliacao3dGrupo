using UnityEngine;

// Cria grupos de moedas em uma faixa aleatoria, de tempos em tempos.
public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public float laneDistance = 2.5f;       // igual ao do PlayerRunner
    public float spawnZ = 40f;
    public float coinHeight = 1.1f;         // altura do centro da moeda

    [Header("Grupos")]
    public int minCoins = 3;
    public int maxCoins = 5;
    public float spacing = 2f;              // distancia entre moedas do mesmo grupo

    [Header("Frequencia (em metros entre grupos)")]
    public float minGap = 25f;
    public float maxGap = 45f;

    float timer = 2f;

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || !gm.IsPlaying || coinPrefab == null) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        int lane = Random.Range(-1, 2);
        int count = Random.Range(minCoins, maxCoins + 1);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(lane * laneDistance, coinHeight, spawnZ + i * spacing);
            Instantiate(coinPrefab, pos, coinPrefab.transform.rotation);
        }

        // converte a distancia em tempo, assim a frequencia se mantem quando o jogo acelera
        timer = Random.Range(minGap, maxGap) / gm.Speed;
    }
}