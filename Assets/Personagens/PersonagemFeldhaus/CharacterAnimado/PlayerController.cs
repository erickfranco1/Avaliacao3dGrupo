using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// Controla movimento básico + parâmetros do Animator usando o novo Input System.
///
/// PARÂMETROS A CRIAR NO ANIMATOR CONTROLLER (nomes exatos):
///   Speed       (Float)   -> velocidade horizontal real do personagem
///   IsMoving    (Bool)    -> true enquanto o jogador segura o input de movimento
///   IsGrounded  (Bool)    -> true quando o personagem está no chão
///   Jump        (Trigger) -> disparado quando o jogador aperta pular
///   Die         (Trigger) -> disparado quando o jogador aperta o botão de morrer
///   IsDead      (Bool)    -> true enquanto a animação de morte está rodando
///
/// ESTADOS: Idle, Run, RunStop, Jump, Die
///
/// Enquanto IsDead for true, TODOS os inputs (mover, pular, morrer de novo) ficam travados.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Animator animator;

    [Header("Inputs (Novo Input System)")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction jumpAction;
    [FormerlySerializedAs("attackAction")]
    [SerializeField] private InputAction dieAction;

    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 20f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float inputDeadzone = 0.1f;

    [Header("Regras de pulo")]
    [Tooltip("Se ligado, só pula quando estiver correndo (como no seu ciclo: Run -> Jump).")]
    [SerializeField] private bool onlyJumpWhileRunning = true;

    [Header("Morte / Travamento de input")]
    [Tooltip("Tempo em segundos que os inputs ficam travados (use o tamanho do seu clip de Die).")]
    [FormerlySerializedAs("attackDuration")]
    [SerializeField] private float dieDuration = 1.5f;
    [Tooltip("Se ligado, permite acionar a morte no ar.")]
    [FormerlySerializedAs("canAttackInAir")]
    [SerializeField] private bool canDieInAir = false;

    // ---- Nomes dos parâmetros do Animator ----
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private bool wasGrounded = true;

    // true enquanto a animação de morte está rodando (inputs travados)
    private bool isDead;
    

    // Chamado ao adicionar o componente: já cria bindings padrão no Inspector.
    private void Reset()
    {
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddBinding("<Gamepad>/leftStick");

        jumpAction = new InputAction("Jump", InputActionType.Button);
        jumpAction.AddBinding("<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        dieAction = new InputAction("Die", InputActionType.Button);
        dieAction.AddBinding("<Mouse>/leftButton");
        dieAction.AddBinding("<Keyboard>/j");
        dieAction.AddBinding("<Gamepad>/buttonWest");
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        dieAction.Enable();
        jumpAction.performed += OnJump;
        dieAction.performed += OnDie;
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJump;
        dieAction.performed -= OnDie;
        moveAction.Disable();
        jumpAction.Disable();
        dieAction.Disable();
    }

    private void Update()
    {
        // Lê o input de movimento; se estiver travado, ignora totalmente
        moveInput =  isDead ? Vector2.zero : moveAction.ReadValue<Vector2>();
        bool hasInput = moveInput.magnitude > inputDeadzone;

        HandleHorizontal(hasInput);
        HandleVertical();

        Vector3 motion = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(motion * Time.deltaTime);

        UpdateAnimator(hasInput);
    }

    private void HandleHorizontal(bool hasInput)
    {
        Vector3 targetVelocity = Vector3.zero;

        if (hasInput)
        {
            Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
            targetVelocity = dir * moveSpeed;

            // Gira o personagem para a direção do movimento
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        float rate = hasInput ? acceleration : deceleration;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * Time.deltaTime);
    }

    private void HandleVertical()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f; // mantém colado no chão

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isDead) return;                 // input travado
        if (!controller.isGrounded) return;

        bool isRunning = moveInput.magnitude > inputDeadzone;
        if (onlyJumpWhileRunning && !isRunning) return;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        animator.SetTrigger(JumpHash);
    }

    private void OnDie(InputAction.CallbackContext ctx)
    {
        if (isDead) return;                 // já está rodando a animação
        if (!canDieInAir && !controller.isGrounded) return;

        isDead = true;
        animator.SetBool(IsDeadHash, true);
        animator.SetTrigger(DieHash);

        CancelInvoke(nameof(EndDie));
        Invoke(nameof(EndDie), dieDuration);
    }

    /// <summary>
    /// Destrava os inputs. Chamado automaticamente após dieDuration,
    /// ou você pode chamar por um Animation Event no final do clip de Die.
    /// </summary>
    public void EndDie()
    {
        CancelInvoke(nameof(EndDie));
        isDead = false;
        animator.SetBool(IsDeadHash, false);
        animator.ResetTrigger(DieHash);
    }

    private void UpdateAnimator(bool hasInput)
    {
        bool grounded = controller.isGrounded;

        animator.SetFloat(SpeedHash, horizontalVelocity.magnitude);
        animator.SetBool(IsMovingHash, hasInput);
        animator.SetBool(IsGroundedHash, grounded);

        // Evita o trigger ficar "preso" caso o pulo seja disparado e a transição não ocorra
        if (grounded && !wasGrounded)
            animator.ResetTrigger(JumpHash);

        wasGrounded = grounded;
    }
}
