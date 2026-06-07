using System;
using UnityEngine;

public sealed class DifficultyDirector : MonoBehaviour
{
    public static DifficultyDirector Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AIDifficultySystem ai;

    [Header("Observation")]
    [SerializeField] private int payoutWindow = 12;
    [SerializeField] private int decideEverySpins = 1;

    [Header("Episode/Reward Targets")]
    [SerializeField] private int initialMoney = 100;
    [SerializeField] private float playableMinNorm = 0.25f;
    [SerializeField] private float playableMaxNorm = 2.50f;

    private int spins;
    private int winStreak;
    private int lossStreak;
    private float sessionSeconds;

    private float[] payoutRing;
    private int payoutIdx;
    private int payoutCount;
    private float payoutSum;

    private float lastStepReward;

    public event Action<ItemEffectType> OnRequestedItemSpawn;

    public bool IsReady => gameManager != null && ai != null;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (!gameManager) gameManager = GameManager.Instance;
        if (!ai) ai = AIDifficultySystem.Instance;

        payoutRing = new float[Mathf.Max(1, payoutWindow)];
        payoutIdx = 0;
        payoutCount = 0;
        payoutSum = 0f;

        spins = 0;
        winStreak = 0;
        lossStreak = 0;
        sessionSeconds = 0f;
        lastStepReward = 0f;
    }

    void Update()
    {
        sessionSeconds += Time.deltaTime;
    }

    public void NotifySpinResolved(int finalPayoutAfterAllAdjustments)
    {
        spins++;

        bool win = finalPayoutAfterAllAdjustments > 0;
        if (win) { winStreak++; lossStreak = 0; }
        else { lossStreak++; winStreak = 0; }

        PushPayout(finalPayoutAfterAllAdjustments);

        if (ai) ai.AdjustAfterSpin(win);

        lastStepReward = ComputeRewardForLastSpin(finalPayoutAfterAllAdjustments);
    }

    public bool ShouldAgentDecideNow()
    {
        int k = Mathf.Max(1, decideEverySpins);
        return (spins % k) == 0;
    }

    public float ConsumeLastStepReward()
    {
        float r = lastStepReward;
        lastStepReward = 0f;
        return r;
    }

    public void GetObservations(out float bankrollNorm,
                                out float winStreakNorm,
                                out float lossStreakNorm,
                                out float sessionNorm,
                                out float freeSpinsNorm,
                                out float recentPayoutMeanNorm,
                                out float difficulty01,
                                out float luckSigned)
    {
        int money = gameManager ? gameManager.Money : initialMoney;
        bankrollNorm = Normalize01(money, 0, Mathf.Max(1, initialMoney * 5));

        winStreakNorm = Mathf.Clamp01(winStreak / 10f);
        lossStreakNorm = Mathf.Clamp01(lossStreak / 10f);

        sessionNorm = Mathf.Clamp01(sessionSeconds / 600f);

        int freeSpins = gameManager ? gameManager.FreeSpins : 0;
        freeSpinsNorm = Mathf.Clamp01(freeSpins / 20f);

        float mean = (payoutCount > 0) ? (payoutSum / payoutCount) : 0f;
        recentPayoutMeanNorm = NormalizeSigned(mean, 100f);

        difficulty01 = ai ? ai.difficulty : 0f;
        luckSigned = ai ? ai.playerLuck : 0f;
    }

    public void ApplyDecision(int difficultyDeltaAction,
                              int luckDeltaAction,
                              int debtTuningAction,
                              int payoutTuningAction,
                              int itemAction,
                              int protectionAction)
    {
        if (!ai) return;

        float dDelta = difficultyDeltaAction switch
        {
            0 => -0.03f,
            1 => 0f,
            2 => 0.03f,
            _ => 0f
        };

        float lDelta = luckDeltaAction switch
        {
            0 => -0.05f,
            1 => 0f,
            2 => 0.05f,
            _ => 0f
        };

        float debtMult = debtTuningAction switch
        {
            0 => 0.9f,
            1 => 1.0f,
            2 => 1.15f,
            _ => 1.0f
        };

        float payoutMult = payoutTuningAction switch
        {
            0 => 0.9f,
            1 => 1.0f,
            2 => 1.15f,
            _ => 1.0f
        };

        bool protectionOn = protectionAction == 1;

        ai.ApplyMacroSettings(dDelta, lDelta, debtMult, payoutMult, protectionOn);

        if (itemAction != 0)
        {
            ItemEffectType t = itemAction switch
            {
                1 => ItemEffectType.IncreaseLuck,
                2 => ItemEffectType.FreeSpins,
                3 => ItemEffectType.GuaranteedWin,
                _ => ItemEffectType.IncreaseLuck
            };
            OnRequestedItemSpawn?.Invoke(t);
        }
    }

    private void PushPayout(int payout)
    {
        if (payoutCount < payoutRing.Length)
        {
            payoutRing[payoutIdx] = payout;
            payoutSum += payout;
            payoutCount++;
            payoutIdx = (payoutIdx + 1) % payoutRing.Length;
            return;
        }

        payoutSum -= payoutRing[payoutIdx];
        payoutRing[payoutIdx] = payout;
        payoutSum += payoutRing[payoutIdx];
        payoutIdx = (payoutIdx + 1) % payoutRing.Length;
    }

    private float ComputeRewardForLastSpin(int finalPayout)
    {
        float r = 0f;

        int money = gameManager ? gameManager.Money : initialMoney;
        float bankrollNorm = (float)money / Mathf.Max(1, initialMoney);

        bool inPlayable = bankrollNorm >= playableMinNorm && bankrollNorm <= playableMaxNorm;

        r += 0.02f;
        if (inPlayable) r += 0.06f;

        if (money <= 0) r -= 2.0f;

        if (bankrollNorm > playableMaxNorm) r -= 0.06f;
        if (bankrollNorm < playableMinNorm) r -= 0.06f;

        if (Mathf.Abs(finalPayout) >= 100) r -= 0.08f;

        float d = ai ? ai.difficulty : 0f;
        if (d > 0.9f) r -= 0.02f;

        return r;
    }

    private static float Normalize01(float v, float min, float max)
    {
        if (max <= min) return 0f;
        return Mathf.Clamp01((v - min) / (max - min));
    }

    private static float NormalizeSigned(float v, float scale)
    {
        if (scale <= 0f) return 0f;
        return Mathf.Clamp(v / scale, -1f, 1f);
    }
}
