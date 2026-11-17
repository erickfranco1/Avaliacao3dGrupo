using System.Collections.Generic;
using UnityEngine;

// Moeda coletavel que vem em direcao ao jogador. Gira sozinha e soma pontos ao ser coletada.
// O Rigidbody e o trigger sao configurados pelo proprio script.
[RequireComponent(typeof(Rigidbody))]
public class Coin : MonoBehaviour
{
    // Lista de todas as moedas ativas. O RigTargets usa para achar a mais proxima.
    public static readonly List<Coin> All = new List<Coin>();

    public int value = 1;
    public float spinSpeed = 180f;
    public float destroyZ = -10f;

    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.isTrigger = true;
    }

    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    // Moeda mais proxima que esta na frente do jogador, dentro da distancia maxima
    public static Coin NearestAhead(Vector3 from, float maxDistance)
    {
        Coin best = null;
        float bestDz = float.MaxValue;

        foreach (var c in All)
        {
            float dz = c.transform.position.z - from.z;
            if (dz < 0.5f || dz > maxDistance) continue;
            if (dz < bestDz)
            {
                bestDz = dz;
                best = c;
            }
        }
        return best;
    }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        var gm = GameManager.Instance;
        if (gm == null || !gm.IsPlaying) return;

        transform.Translate(Vector3.back * gm.Speed * Time.deltaTime, Space.World);

        if (transform.position.z < destroyZ)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.AddCoin(value);
        Destroy(gameObject);
    }
}