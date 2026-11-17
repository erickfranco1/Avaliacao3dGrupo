using UnityEngine;

// Camera do menu: no Menu fica de frente para o personagem (para a mao do IK ficar visivel).
// Ao comecar o jogo, faz um arco ate a posicao de corrida, atras do personagem.
// A posicao de corrida e a que a camera tem na cena ao dar Play.
public class MenuCamera : MonoBehaviour
{
    [Header("Posicao no menu (de frente para o personagem)")]
    public Vector3 menuPosition = new Vector3(0f, 1.3f, 3f);
    public Vector3 menuEuler = new Vector3(5f, 180f, 0f);

    [Header("Transicao")]
    public float duration = 1.2f;
    [Tooltip("Largura do arco da camera ao passar pelo lado do personagem, para nao atravessar o corpo.")]
    public float arcWidth = 3f;

    Vector3 gamePosition;
    Vector3 gameEuler;
    float blend;                            // 0 = menu, 1 = corrida
    bool first = true;

    void Awake()
    {
        // A pose de corrida e a que a camera tem na cena
        gamePosition = transform.position;
        gameEuler = transform.eulerAngles;
    }

    void LateUpdate()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        float target = gm.State == GameState.Menu ? 0f : 1f;

        // no primeiro frame vai direto para a pose certa (ex.: depois de reiniciar a partida)
        if (first)
        {
            blend = target;
            first = false;
        }
        else
        {
            blend = Mathf.MoveTowards(blend, target, Time.deltaTime / duration);
        }

        float e = Mathf.SmoothStep(0f, 1f, blend);

        // posicao em arco, passando pelo lado
        Vector3 pos = Vector3.Lerp(menuPosition, gamePosition, e);
        pos += Vector3.right * Mathf.Sin(Mathf.PI * e) * arcWidth;

        // rotacao: o yaw gira sempre no mesmo sentido, para a camera continuar olhando o personagem no arco
        float gameYaw = gameEuler.y;
        if (gameYaw <= menuEuler.y) gameYaw += 360f;
        float yaw = Mathf.Lerp(menuEuler.y, gameYaw, e);
        float pitch = Mathf.LerpAngle(menuEuler.x, gameEuler.x, e);
        float roll = Mathf.LerpAngle(menuEuler.z, gameEuler.z, e);

        transform.SetPositionAndRotation(pos, Quaternion.Euler(pitch, yaw, roll));
    }
}