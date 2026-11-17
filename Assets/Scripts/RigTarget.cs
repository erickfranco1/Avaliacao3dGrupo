using UnityEngine;
using UnityEngine.Animations.Rigging;

// Controla os alvos e o peso do Rig. Fica no objeto raiz do personagem
// (o mesmo que tem o Animator e o Rig Builder).
public class RigTargets : MonoBehaviour
{
    [Header("Rig")]
    public Rig rig;                         // o objeto filho "Rig"
    public Animator animator;               // se vazio, busca sozinho
    [Tooltip("Nome do estado de corrida no Animator. O rig so atua nele.")]
    public string runStateName = "FloweryRun";
    public float rigFadeSpeed = 6f;

    [Header("Cabeca")]
    public Transform headTarget;            // alvo do Multi-Aim
    public float lookDistance = 14f;        // so olha para moedas ate essa distancia
    public float headTargetSmooth = 8f;
    [Tooltip("Para onde olha quando nao ha moeda (relativo ao personagem).")]
    public Vector3 restPoint = new Vector3(0f, 1.6f, 8f);

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        UpdateRigWeight();
        UpdateHeadTarget();
    }

    // O rig so vale durante a corrida. No flip, no slide e na morte o peso vai a 0 suavemente.
    void UpdateRigWeight()
    {
        if (rig == null || animator == null) return;

        var state = animator.GetCurrentAnimatorStateInfo(0);
        bool running = state.IsName(runStateName) && !animator.IsInTransition(0);

        rig.weight = Mathf.MoveTowards(rig.weight, running ? 1f : 0f, rigFadeSpeed * Time.deltaTime);
    }

    // Move o alvo da cabeca para a moeda mais proxima, ou para a frente se nao houver nenhuma
    void UpdateHeadTarget()
    {
        if (headTarget == null) return;

        Vector3 desired = transform.TransformPoint(restPoint);

        Coin coin = Coin.NearestAhead(transform.position, lookDistance);
        if (coin != null) desired = coin.transform.position;

        headTarget.position = Vector3.Lerp(headTarget.position, desired, headTargetSmooth * Time.deltaTime);
    }
}