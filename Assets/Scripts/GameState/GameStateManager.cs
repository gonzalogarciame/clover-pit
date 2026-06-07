using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameState
{
    Menu,
    Playing,
    Win,
    GameOver
}

public enum GameOverReason
{
    None,
    NoSpinsLeft,
    Bankrupt
}


public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject endPanel;

    [Header("End Panel")]
    [SerializeField] private TMP_Text endTitleText;
    [SerializeField] private Button continueButton;

    [Header("HUD Buttons (optional)")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button storeButton;

    [Header("Flow")]
    [SerializeField] private bool resetRunOnPlay = true;

    [SerializeField] private LeverDrag lever;


    public GameOverReason LastGameOverReason { get; private set; } = GameOverReason.None;

    public GameState CurrentState { get; private set; }

    private bool lastEndWasWin;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ChangeState(GameState.Menu);
    }



    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        menuPanel?.SetActive(false);
        hudPanel?.SetActive(false);
        endPanel?.SetActive(false);

        switch (newState)
        {
            case GameState.Menu:
                menuPanel?.SetActive(true);
                break;

            case GameState.Playing:
                LastGameOverReason = GameOverReason.None; // ← AQUÍ
                hudPanel?.SetActive(true);


                if (resetRunOnPlay)
                    GameManager.Instance.ResetRunValues();

                break;

            case GameState.Win:
                ShowWinEndPanel();
                break;

            case GameState.GameOver:
                ShowLoseEndPanel();
                break;
        }
    }


    private void ShowWinEndPanel()
    {
        lastEndWasWin = true;

        endPanel?.SetActive(true);
        endTitleText.text = "YOU WIN";

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);

        lever?.ForceResetImmediate();


        hudPanel?.SetActive(false);
    }

    private void ShowLoseEndPanel()
    {
        lastEndWasWin = false;

        endPanel?.SetActive(true);

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        lever?.ForceResetImmediate();

        switch (LastGameOverReason)
        {
            case GameOverReason.Bankrupt:
                endTitleText.text = "BANKRUPT";
                break;
            case GameOverReason.NoSpinsLeft:
                endTitleText.text = "NO SPINS";
                break;
            default:
                endTitleText.text = "YOU LOSE";
                break;
        }

        hudPanel?.SetActive(false);
    }



    public void ContinueAfterWin()
    {
        if (!lastEndWasWin) return;

        GameManager.Instance.ApplyWinContinue();

        endPanel?.SetActive(false);
        hudPanel?.SetActive(true);

        CurrentState = GameState.Playing;

        UIManager.Instance?.ShowEffectMessage(
            $"CONTINUAS → Deuda x{GameManager.Instance.DebtMultiplier} | Coste: ${GameManager.Instance.CurrentSpinCost}"
        );
    }

    public void OnPlayPressed()
    {
        ChangeState(GameState.Playing);
        LastGameOverReason = GameOverReason.None;
    }

    public void OnRestartPressed()
    {
        GameManager.Instance.RestartGame();
        ChangeState(GameState.Playing);
    }   

    public void OnMenuPressed()
    {
        ChangeState(GameState.Menu);
    }
    public void ExitGame()
    {
        Debug.Log("Exit Game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void GameOver(GameOverReason reason)
    {
        LastGameOverReason = reason;
        ChangeState(GameState.GameOver);
    }

}
