using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RigTargets : MonoBehaviour
{
    [Header("Rig")]
    public Rig rig;
    public Animator animator;
    public string runStateName = "FloweryRun";
    public float rigFadeSpeed = 6f;

    [Header("Cabeca")]
    public Transform headTarget;
    public float lookDistance = 14f;
    public float headTargetSmooth = 8f;
    
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