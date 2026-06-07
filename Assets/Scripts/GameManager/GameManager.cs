using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Menu,
        Play,
        End
    }

    [Header("Panels / Gameplay Roots")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject leverRoot;

    [Header("Starting Values")]
    [SerializeField] private int startingMoney = 1000;

    [Header("Run Rules")]
    [SerializeField] private int spinsPerLevel = 15;          // tiradas base del nivel
    [SerializeField] private int baseSpinCost = 10;           // coste base (deuda x1)
    [SerializeField] private int debtMultiplierStep = 2;     // cada continue multiplica x10
    [SerializeField] private int baseGoalMoney = 1000;        // objetivo base al que luego se le sumara 250

    [Header("Incremental Progression")]
    [SerializeField] private int spinsIncreasePerContinue = 2;   // + tiradas por continue
    [SerializeField] private int goalIncreasePerContinue = 250;  // + objetivo base por continue
    private int runIndex = 0;                                     // 0=primera run, 1=tras 1 continue, etc.

    public float Mult3 { get; private set; } = 1f;
    public float Mult4 { get; private set; } = 1f;
    public float Mult5 { get; private set; } = 1f;

    public void Upgrade3(float add) => Mult3 += add;
    public void Upgrade4(float add) => Mult4 += add;
    public void Upgrade5(float add) => Mult5 += add;

    public int StartMoney { get; private set; }

    public int Money { get; private set; }
    public int FreeSpins { get; private set; }

    public int DebtMultiplier { get; private set; } = 1;

    public int SpinsLeft { get; private set; }

    // Coste actual por tirada pagada
    public int CurrentSpinCost => baseSpinCost * DebtMultiplier;

    // Valores incrementales por continue
    private int CurrentSpinsPerLevel => spinsPerLevel + runIndex * spinsIncreasePerContinue;
    private int CurrentBaseGoalMoney => baseGoalMoney + runIndex * goalIncreasePerContinue;


    public int CurrentGoalMoney
    {
        get
        {
            int goal = CurrentBaseGoalMoney;

            return goal + goalIncreasePerContinue;
        }
    }

    public bool IsDoubleWinsActive { get; set; }
    public bool IsGuaranteedWinActive { get; set; }

    public bool IsGameOver { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Menu;

    public event Action OnSpinFinished;

    public void NotifySpinFinished()
    {
        OnSpinFinished?.Invoke();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Inicialización de partida
        Money = startingMoney;
        StartMoney = Money;     
        FreeSpins = 0;

        ResetRunValues(); 

        // Estado inicial: menú -> lever oculto
        SetState(GameState.Menu);

        // UI
        UIManager.Instance?.RefreshMoney(Money);
        UIManager.Instance?.RefreshFreeSpins(FreeSpins);
        UIManager.Instance?.RefreshRoundUI(DebtMultiplier, SpinsLeft, CurrentSpinCost, CurrentGoalMoney);
    }


    public void SetState(GameState newState)
    {
        CurrentState = newState;

        if (menuPanel) menuPanel.SetActive(newState == GameState.Menu);
        if (endPanel) endPanel.SetActive(newState == GameState.End);

        // LeverRoot solo visible en PLAY
        if (leverRoot) leverRoot.SetActive(newState == GameState.Play);
    }

    public void GoToMenu() => SetState(GameState.Menu);
    public void StartPlay() => SetState(GameState.Play);
    public void ShowEnd() => SetState(GameState.End);


    public void ResetRunValues()
    {
        runIndex = 0;
        DebtMultiplier = 1;
        SpinsLeft = CurrentSpinsPerLevel;
        IsGameOver = false;

        UIManager.Instance?.RefreshRoundUI(DebtMultiplier, SpinsLeft, CurrentSpinCost, CurrentGoalMoney);
    }


    public void ApplyWinContinue()
    {
        if (IsGameOver) return;

        runIndex++;
        DebtMultiplier *= debtMultiplierStep;
        SpinsLeft = CurrentSpinsPerLevel;

        UIManager.Instance?.RefreshRoundUI(DebtMultiplier, SpinsLeft, CurrentSpinCost, CurrentGoalMoney);
    }


    public bool TryConsumeSpinAttempt()
    {
        if (IsGameOver) return false;
        if (SpinsLeft <= 0) return false;

        SpinsLeft--;
        UIManager.Instance?.RefreshRoundUI(DebtMultiplier, SpinsLeft, CurrentSpinCost, CurrentGoalMoney);

        if (SpinsLeft <= 0)
        {
            IsGameOver = true;

        }

        return true;
    }


    public void AddMoney(int amount)
    {
        if (amount > 0 && IsDoubleWinsActive)
            amount *= 2;

        Money += amount;

        UIManager.Instance?.RefreshMoney(Money);
        UIManager.Instance?.RefreshRoundUI(DebtMultiplier, SpinsLeft, CurrentSpinCost, CurrentGoalMoney);
    }

    public void AddFreeSpins(int spins)
    {
        FreeSpins += Mathf.Max(0, spins);
        UIManager.Instance?.RefreshFreeSpins(FreeSpins);
    }

    public bool UseFreeSpin()
    {
        if (FreeSpins <= 0) return false;

        FreeSpins--;
        UIManager.Instance?.RefreshFreeSpins(FreeSpins);
        return true;
    }


    public bool CanAffordCurrentSpin()
    {
        return Money >= CurrentSpinCost;
    }

    public void RestartGame()
    {
        Money = startingMoney;
        StartMoney = Money;
        FreeSpins = 0;

        IsDoubleWinsActive = false;
        IsGuaranteedWinActive = false;

        runIndex = 0;
        DebtMultiplier = 1;
        SpinsLeft = CurrentSpinsPerLevel;

        IsGameOver = false;

        SetState(GameState.Menu);

        UIManager.Instance?.RefreshMoney(Money);
        UIManager.Instance?.RefreshFreeSpins(FreeSpins);
        UIManager.Instance?.RefreshRoundUI(DebtMultiplier, SpinsLeft, CurrentSpinCost, CurrentGoalMoney);
    }

    private Dictionary<Symbol, float> symbolBaseBonus = new();

    public void AddSymbolBaseBonus(Symbol symbol, float bonus)
    {
        if (symbol == null) return;

        if (!symbolBaseBonus.ContainsKey(symbol))
            symbolBaseBonus[symbol] = 0f;

        symbolBaseBonus[symbol] += bonus;
    }

    public float GetSymbolBaseBonus(Symbol symbol)
    {
        return symbolBaseBonus.TryGetValue(symbol, out var b) ? b : 0f;
    }
}
