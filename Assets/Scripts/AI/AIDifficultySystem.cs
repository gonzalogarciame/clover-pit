using System.Collections;
using UnityEngine;

public class AIDifficultySystem : MonoBehaviour
{
    public static AIDifficultySystem Instance { get; private set; }

    [Range(0f, 1f)] public float baseWinChance = 0.4f;
    private bool forceWinNextSpin;
    private bool forceLoseNextSpin;

    [Range(0f, 1f)] public float difficulty = 0.0f;
    [Range(-0.5f, 0.5f)] public float playerLuck = 0.0f;

    [Header("Debt / Cost")]
    [SerializeField] private float debtMultiplier = 1.0f;
    [SerializeField] private bool lossProtectionEnabled = false;
    [SerializeField] private int maxLossPerSpin = 15;

    [Header("Payout")]
    [SerializeField] private float payoutMultiplier = 1.0f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ApplyMacroSettings(float difficultyDelta, float luckDelta, float newDebtMultiplier, float newPayoutMultiplier, bool enableLossProtection)
    {
        difficulty = Mathf.Clamp01(difficulty + difficultyDelta);
        playerLuck = Mathf.Clamp(playerLuck + luckDelta, -0.5f, 0.5f);

        debtMultiplier = Mathf.Clamp(newDebtMultiplier, 0.75f, 1.5f);
        payoutMultiplier = Mathf.Clamp(newPayoutMultiplier, 0.75f, 1.5f);

        lossProtectionEnabled = enableLossProtection;
    }

    public int GetSpinCost(int baseCost)
    {
        float diffCost = Mathf.Lerp(1.0f, 1.6f, difficulty);
        float final = baseCost * diffCost * debtMultiplier;
        return Mathf.RoundToInt(final);
    }

    public int ApplyPayoutDifficulty(int rawPayout)
    {
        if (rawPayout <= 0) return rawPayout;

        float diffPayout = Mathf.Lerp(1.0f, 0.65f, difficulty);
        float final = rawPayout * diffPayout * payoutMultiplier;
        return Mathf.RoundToInt(final);
    }

    public int ClampLoss(int negativeAmount)
    {
        if (!lossProtectionEnabled) return negativeAmount;
        if (negativeAmount >= 0) return negativeAmount;
        return Mathf.Max(negativeAmount, -Mathf.Abs(maxLossPerSpin));
    }

    public float GetLuckWeightFactor()
    {
        return Mathf.Clamp(1f + playerLuck, 0.5f, 1.5f);
    }

    public void AdjustAfterSpin(bool win)
    {
        if (win)
        {
            difficulty = Mathf.Clamp01(difficulty + 0.03f);
            playerLuck = Mathf.Clamp(playerLuck - 0.02f, -0.5f, 0.5f);
        }
        else
        {
            difficulty = Mathf.Clamp01(difficulty - 0.02f);
            playerLuck = Mathf.Clamp(playerLuck + 0.03f, -0.5f, 0.5f);
        }
    }


    public bool GetSpinResult()
    {
        if (forceWinNextSpin) { forceWinNextSpin = false; return true; }
        if (forceLoseNextSpin) { forceLoseNextSpin = false; return false; }

        float adjustedChance = baseWinChance + playerLuck - difficulty;
        adjustedChance = Mathf.Clamp01(adjustedChance);
        return Random.value < adjustedChance;
    }

    public void ForceNextWin() => forceWinNextSpin = true;
    public void ForceNextLoss() => forceLoseNextSpin = true;

    public void MakeEasier(float amount = 0.05f)
    {
        difficulty = Mathf.Clamp01(difficulty - Mathf.Abs(amount));
    }

    public void MakeHarder(float amount = 0.05f)
    {
        difficulty = Mathf.Clamp01(difficulty + Mathf.Abs(amount));
    }

    public void BoostLuck(float amount = 0.10f)
    {
        playerLuck = Mathf.Clamp(playerLuck + amount, -0.5f, 0.5f);
    }

    public void BoostLuck(float amount, float duration)
    {
        StartCoroutine(BoostLuckRoutine(amount, duration));
    }

    private IEnumerator BoostLuckRoutine(float amount, float duration)
    {
        float original = playerLuck;
        playerLuck = Mathf.Clamp(playerLuck + amount, -0.5f, 0.5f);
        yield return new WaitForSeconds(Mathf.Max(0f, duration));
        playerLuck = original;
    }
}
