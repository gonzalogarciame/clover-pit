using UnityEngine;

public class AIDifficultySystem : MonoBehaviour
{
    public static AIDifficultySystem Instance { get; private set; }

    [Range(0f, 1f)] public float baseWinChance = 0.4f; // probabilidad base de ganar
    [Range(-0.5f, 0.5f)] public float playerLuck = 0.0f;    // modificador dinámico de suerte
    [Range(0f, 1f)] public float difficulty = 0.0f;         // cuanto más alto, más difícil

    // --- Flags para comandos ---
    private bool forceWinNextSpin;
    private bool forceLoseNextSpin;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ==========================
    // API usada por los comandos
    // ==========================

    public void BoostLuck(float amount)
    {
        playerLuck = Mathf.Clamp(playerLuck + amount, -0.5f, 0.5f);
    }

    public void MakeEasier(float amount)
    {
        difficulty = Mathf.Clamp01(difficulty - amount);
    }

    public void MakeHarder(float amount)
    {
        difficulty = Mathf.Clamp01(difficulty + amount);
    }

    public void ForceNextWin()
    {
        forceWinNextSpin = true;
        forceLoseNextSpin = false;
    }

    public void ForceNextLoss()
    {
        forceLoseNextSpin = true;
        forceWinNextSpin = false;
    }

    // ==========================
    // Probabilidad de resultado
    // ==========================
    public bool GetSpinResult()
    {
        // Comandos "hard" tienen prioridad
        if (forceWinNextSpin)
        {
            forceWinNextSpin = false;
            return true;
        }
        if (forceLoseNextSpin)
        {
            forceLoseNextSpin = false;
            return false;
        }

        // Fórmula de probabilidad ajustada
        float adjustedChance = baseWinChance + playerLuck - difficulty;
        adjustedChance = Mathf.Clamp01(adjustedChance);

        bool win = Random.value < adjustedChance;
        return win;
    }

    // Ajuste dinámico del comportamiento (tu lógica base)
    public void AdjustAfterSpin(bool win)
    {
        if (win)
        {
            difficulty += 0.05f; // cada victoria sube la dificultad
            playerLuck -= 0.02f; // reduce la suerte
        }
        else
        {
            difficulty -= 0.02f; // si pierde, baja la dificultad
            playerLuck += 0.05f; // aumenta la suerte
        }

        difficulty = Mathf.Clamp01(difficulty);
        playerLuck = Mathf.Clamp(playerLuck, -0.5f, 0.5f);
    }
}
