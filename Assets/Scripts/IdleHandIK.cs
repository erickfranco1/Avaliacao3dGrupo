using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

// IK da mao no menu: quando o personagem esta parado (Idle), a mao acompanha o mouse.
// Usa um Rig proprio (RigIdle), separado do Rig da corrida. O peso sobe so no Idle.
[DefaultExecutionOrder(100)]
public class IdleHandIK : MonoBehaviour
{
    [Header("Rig do menu (contem o Two Bone IK do braco)")]
    public Rig rigIdle;
    public Transform handTarget;
    public Transform hint;                  // guia a direcao do cotovelo
    public Transform shoulderBone;          // mixamorig:RightArm (ou LeftArm)
    public Animator animator;
    public Camera cam;                      // se vazio, usa a Main Camera

    [Header("Estado")]
    public string idleStateName = "Idle";
    public float rigFadeSpeed = 4f;

    [Header("Alcance")]
    public float reach = 0.6f;              // distancia maxima da mao ate o ombro
    public float planeDistance = 0.2f;      // distancia do plano alvo a frente do personagem
    public float planeHeight = 1.3f;
    public float targetSmooth = 12f;
    [Tooltip("Posicao do cotovelo em relacao ao ombro, no espaco do personagem. Para o braco esquerdo use X negativo.")]
    public Vector3 hintOffset = new Vector3(0.4f, -0.3f, -0.5f);

    Vector3 smoothedTarget;
    bool hasTarget;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (rigIdle == null || animator == null) return;
        if (cam == null) cam = Camera.main;

        // O IK so vale no Idle (menu). Ao comecar a correr, o peso cai suavemente.
        var state = animator.GetCurrentAnimatorStateInfo(0);
        bool idle = state.IsName(idleStateName) && !animator.IsInTransition(0);
        rigIdle.weight = Mathf.MoveTowards(rigIdle.weight, idle ? 1f : 0f, rigFadeSpeed * Time.deltaTime);

        if (rigIdle.weight <= 0f || cam == null || Mouse.current == null
            || shoulderBone == null || handTarget == null) return;

        // Raio do mouse ate um plano vertical perto do personagem, virado para a camera
        Vector3 planePoint = transform.position + transform.forward * planeDistance + Vector3.up * planeHeight;
        var plane = new Plane(-transform.forward, planePoint);
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!plane.Raycast(ray, out float dist)) return;
        Vector3 hit = ray.GetPoint(dist);

        // Limita ao alcance do braco, para o IK nao esticar demais
        Vector3 fromShoulder = Vector3.ClampMagnitude(hit - shoulderBone.position, reach);
        Vector3 desired = shoulderBone.position + fromShoulder;

        smoothedTarget = hasTarget ? Vector3.Lerp(smoothedTarget, desired, targetSmooth * Time.deltaTime) : desired;
        hasTarget = true;
        handTarget.position = smoothedTarget;

        if (hint != null)
            hint.position = transform.TransformPoint(transform.InverseTransformPoint(shoulderBone.position) + hintOffset);
    }
}