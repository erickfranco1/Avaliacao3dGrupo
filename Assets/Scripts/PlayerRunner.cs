using UnityEngine;
using UnityEngine.InputSystem;

// Corredor de 3 faixas: A/D (ou setas) trocam de faixa, Espaco/W pula, S desliza.
// Animator esperado: bools isRunning, isSliding, isDead e trigger Jump.
[RequireComponent(typeof(CharacterController))]
public class PlayerRunner : MonoBehaviour
{
    [Header("Faixas")]
    public float laneDistance = 2.5f;
    public float laneChangeSpeed = 14f;

    [Header("Pulo")]
    public float jumpHeight = 1.6f;
    public float gravity = -28f;
    public float fastFallSpeed = -20f;      // S no ar = cair rapido

    [Header("Deslize")]
    public float slideDuration = 0.8f;
    [Range(0.3f, 0.8f)] public float slideHeightRatio = 0.5f;

    CharacterController cc;
    Animator anim;
    int lane = 1;                           // 0 esquerda, 1 meio, 2 direita
    float velY;
    bool active, sliding;
    float slideTimer;
    float standHeight;
    Vector3 standCenter;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        anim.applyRootMotion = false;
        standHeight = cc.height;
        standCenter = cc.center;
    }

    public void StartRun()
    {
        active = true;
        anim.SetBool("isRunning", true);
    }

    public void Die()
    {
        active = false;
        StopSlide();
        anim.SetBool("isRunning", false);
        anim.SetBool("isDead", true);
    }

    void Update()
    {
        var kb = Keyboard.current;
        bool grounded = cc.isGrounded;

        if (active && kb != null)
        {
            if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
                lane = Mathf.Max(0, lane - 1);
            if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
                lane = Mathf.Min(2, lane + 1);

            bool jump = kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
            bool slide = kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame;

            if (jump && grounded)
            {
                StopSlide();
                velY = Mathf.Sqrt(jumpHeight * -2f * gravity);
                anim.SetTrigger("Jump");
            }

            if (slide)
            {
                if (grounded) StartSlide();
                else velY = fastFallSpeed;
            }
        }

        if (sliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f) StopSlide();
        }

        if (grounded && velY < 0f) velY = -2f;
        velY += gravity * Time.deltaTime;

        float targetX = (lane - 1) * laneDistance;
        float dx = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime) - transform.position.x;
        cc.Move(new Vector3(dx, velY * Time.deltaTime, 0f));
    }

    void StartSlide()
    {
        if (sliding) return;
        sliding = true;
        slideTimer = slideDuration;
        cc.height = standHeight * slideHeightRatio;
        cc.center = new Vector3(standCenter.x, standCenter.y * slideHeightRatio, standCenter.z);
        anim.SetBool("isSliding", true);
    }

    void StopSlide()
    {
        if (!sliding) return;
        sliding = false;
        cc.height = standHeight;
        cc.center = standCenter;
        anim.SetBool("isSliding", false);
    }
}