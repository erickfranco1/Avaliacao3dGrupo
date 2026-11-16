using UnityEngine;

// Obstaculo que vem em direcao ao jogador. Qualquer objeto com Collider vira obstaculo.
// O proprio script configura Rigidbody kinematic e trigger, para nao dar trabalho no Inspector.
[RequireComponent(typeof(Rigidbody))]
public class Obstacle : MonoBehaviour
{
    [Tooltip("Altura da base do obstaculo. 0 = no chao (pular). ~1.1 = no alto (deslizar).")]
    public float bottomY = 0f;
    public float destroyZ = -10f;

    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.isTrigger = true;
    }

    void Start()
    {
        // Posiciona a base do objeto em bottomY, nao importa onde esta o pivot do modelo
        var r = GetComponentInChildren<Renderer>();
        if (r != null)
            transform.position += Vector3.up * (bottomY - r.bounds.min.y);
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || !gm.IsPlaying) return;

        transform.Translate(Vector3.back * gm.Speed * Time.deltaTime, Space.World);

        if (transform.position.z < destroyZ)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.PlayerHit();
    }
}