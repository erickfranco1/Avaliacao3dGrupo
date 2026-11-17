using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Playing, GameOver }

// Game loop: Menu -> Jogando -> Game Over -> Reiniciar.
// A interface usa OnGUI para funcionar sem montar Canvas. Pode ser trocada por UI depois.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referencias")]
    public PlayerRunner player;
    public string gameTitle = "Nome do Jogo";

    [Header("Velocidade do mundo")]
    public float startSpeed = 10f;
    public float maxSpeed = 28f;
    public float acceleration = 0.25f;      // ganho de velocidade por segundo

    [Header("Moedas")]
    public int coinBonus = 10;              // pontos (metros) por moeda

    public GameState State { get; private set; } = GameState.Menu;
    public float Speed { get; private set; }
    public float Score { get; private set; }
    public int Coins { get; private set; }
    public bool IsPlaying => State == GameState.Playing;

    static bool skipMenu;                   // depois de reiniciar, comeca direto
    int best;

    void Awake()
    {
        Instance = this;
        Speed = startSpeed;
        best = PlayerPrefs.GetInt("BestScore", 0);
    }

    void Start()
    {
        if (skipMenu)
        {
            skipMenu = false;
            StartGame();
        }
    }

    void Update()
    {
        var kb = Keyboard.current;
        bool enter = kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame);

        if (State == GameState.Menu && enter) StartGame();
        else if (State == GameState.GameOver && enter) Restart();

        if (State == GameState.Playing)
        {
            Speed = Mathf.Min(maxSpeed, Speed + acceleration * Time.deltaTime);
            Score += Speed * Time.deltaTime;
        }
    }

    public void StartGame()
    {
        State = GameState.Playing;
        player.StartRun();
    }

    public void AddCoin(int value)
    {
        if (State != GameState.Playing) return;
        Coins += value;
        Score += coinBonus * value;
    }

    public void PlayerHit()
    {
        if (State != GameState.Playing) return;
        State = GameState.GameOver;

        if ((int)Score > best)
        {
            best = (int)Score;
            PlayerPrefs.SetInt("BestScore", best);
        }
        player.Die();
    }

    public void Restart()
    {
        skipMenu = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnGUI()
    {
        int fs = Mathf.RoundToInt(Screen.height * 0.045f);
        var label = new GUIStyle(GUI.skin.label)
        {
            fontSize = fs,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter
        };
        label.normal.textColor = Color.black;
        var title = new GUIStyle(label) { fontSize = fs * 2 };
        var button = new GUIStyle(GUI.skin.button) { fontSize = fs };

        float w = Screen.width, h = Screen.height;
        float bw = w * 0.3f, bh = h * 0.1f;

        if (State == GameState.Playing)
        {
            GUI.Label(new Rect(0, 10, w, fs * 2), (int)Score + " m", label);
            GUI.Label(new Rect(0, 10 + fs * 1.6f, w, fs * 2), "Moedas: " + Coins, label);
        }
        else if (State == GameState.Menu)
        {
            GUI.Label(new Rect(0, h * 0.2f, w, fs * 3), gameTitle, title);
            GUI.Label(new Rect(0, h * 0.4f, w, fs * 2), "Recorde: " + best + " m", label);
            if (GUI.Button(new Rect((w - bw) / 2, h * 0.55f, bw, bh), "Jogar (Clicar ou Enter)", button)) StartGame();
        }
        else
        {
            GUI.Label(new Rect(0, h * 0.15f, w, fs * 3), "Game Over", title);
            GUI.Label(new Rect(0, h * 0.35f, w, fs * 4),
                (int)Score + " m\nMoedas: " + Coins + "\nRecorde: " + best + " m", label);
            if (GUI.Button(new Rect((w - bw) / 2, h * 0.65f, bw, bh), "Jogar de novo (Enter)", button)) Restart();
        }
    }
}