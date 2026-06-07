using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SlotMachineGrid : MonoBehaviour
{
    [Header("UI Grid 3x5")]
    [SerializeField] private Transform gridParent;
    [SerializeField] private float spinTime = 0.9f;
    [SerializeField] private float tick = 0.05f;

    [Header("Symbols")]
    [SerializeField] private List<Symbol> symbols;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem winParticleSystem;
    [SerializeField] private ParticleSystem jackpotParticleSystem;

    [Header("Win Anim Timing")]
    [SerializeField] private float delayBetweenGroups = 0.12f;

    private Image[,] cells2D = new Image[3, 5];
    private readonly Symbol[,] result = new Symbol[3, 5];
    private bool isSpinning;



    public bool IsSpinning => isSpinning;

    [Serializable] 
    private class WinningGroup
    {
        public List<(int r, int c)> cells;
        public int payout;
    }

    private readonly List<WinningGroup> lastWinningGroups = new();

    void Awake()
    {
        BuildCellsMap();
    }

    private void BuildCellsMap()
    {
        cells2D = new Image[3, 5];

        var images = gridParent.GetComponentsInChildren<Image>(includeInactive: true)
            .Where(img => img.transform != gridParent)
            .ToArray();

        var used = new HashSet<(int r, int c)>();
        int assigned = 0;

        foreach (var img in images)
        {
            var idx = img.GetComponent<GridCellIndex>();
            if (idx == null) continue;

            int r = idx.row;
            int c = idx.col;

            if (r < 0 || r > 2 || c < 0 || c > 4)
                continue;

            if (!used.Add((r, c)))
                continue;

            cells2D[r, c] = img;
            assigned++;
        }

        if (assigned != 15)
        {
            Debug.LogError($"GridCellIndex incompleto: asignadas {assigned}/15. " +
                           "Cada celda debe tener GridCellIndex con row(0..2), col(0..4) sin duplicados.");
        }

        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 5; c++)
                if (cells2D[r, c] == null)
                    Debug.LogError($"Falta celda en row={r}, col={c}. Revisa GridCellIndex.");
    }

    private Image Cell(int r, int c) => cells2D[r, c];

    public void Spin()
    {
        if (GameStateManager.Instance != null &&
            GameStateManager.Instance.CurrentState != GameState.Playing)
        {
            UIManager.Instance?.ShowEffectMessage("No puedes girar ahora.");
            return;
        }

        if (isSpinning) return;

        if (GameManager.Instance.IsGameOver)
        {
            UIManager.Instance?.ShowEffectMessage("No quedan tiradas. Has perdido.");
            GameStateManager.Instance?.ChangeState(GameState.GameOver);
            return;
        }

        int cost = GameManager.Instance.CurrentSpinCost;

        if (GameManager.Instance.FreeSpins <= 0 && GameManager.Instance.Money < cost)
        {
            UIManager.Instance?.ShowEffectMessage("Sin dinero y sin free spins. Has perdido.");
            GameStateManager.Instance?.GameOver(GameOverReason.Bankrupt);
            return;
        }

        bool usedFreeSpin = GameManager.Instance.UseFreeSpin();

        if (!usedFreeSpin)
        {
            if (GameManager.Instance.Money < cost)
            {
                UIManager.Instance?.ShowEffectMessage("¡Dinero insuficiente!");
                GameStateManager.Instance?.ChangeState(GameState.GameOver);
                return;
            }

            if (!GameManager.Instance.TryConsumeSpinAttempt())
            {
                UIManager.Instance?.ShowEffectMessage("No quedan tiradas. Has perdido.");
                GameStateManager.Instance?.ChangeState(GameState.GameOver);
                return;
            }

            GameManager.Instance.AddMoney(-cost);
            UIManager.Instance?.ShowEffectMessage(
                $"-${cost} por giro | Tiradas: {GameManager.Instance.SpinsLeft}"
            );
        }
        else
        {
            UIManager.Instance?.ShowEffectMessage(
                $"¡Giro gratis usado! | Tiradas: {GameManager.Instance.SpinsLeft}"
            );
        }

        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;

        GenerateResult();

        float t = 0f;
        while (t < spinTime)
        {
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 5; c++)
                {
                    var randSym = symbols[UnityEngine.Random.Range(0, symbols.Count)];
                    Cell(r, c).sprite = randSym.sprite;
                }

            yield return new WaitForSeconds(tick);
            t += tick;
        }

        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 5; c++)
                Cell(r, c).sprite = result[r, c].sprite;

        int payout = EvaluatePayout();

        if (payout > 0)
            yield return StartCoroutine(PlayWinEffects());

        UIManager.Instance?.ShowResultPopup(payout);
        GameManager.Instance.AddMoney(payout);

        bool isJackpot = payout >= 1000;
        bool reachedGoal = GameManager.Instance.Money >= GameManager.Instance.CurrentGoalMoney;

        if (isJackpot || reachedGoal)
            GameStateManager.Instance?.ChangeState(GameState.Win);
        else if (GameManager.Instance.IsGameOver)
            GameStateManager.Instance?.ChangeState(GameState.GameOver);

        isSpinning = false;
        GameManager.Instance?.NotifySpinFinished();
    }

    private IEnumerator PlayWinEffects()
    {
        if (winParticleSystem != null) winParticleSystem.Play();
        yield return StartCoroutine(HighlightWinningSymbols());
    }

    private IEnumerator HighlightWinningSymbols()
    {
        if (lastWinningGroups.Count == 0)
            yield break;

        foreach (var group in lastWinningGroups)
        {
            UIManager.Instance?.ShowResultPopup(group.payout);

            var unique = new HashSet<(int r, int c)>(group.cells);
            var trs = unique.Select(rc => Cell(rc.r, rc.c).transform).ToList();

            bool[] done = new bool[trs.Count];

            for (int i = 0; i < trs.Count; i++)
            {
                int localI = i;
                StartCoroutine(PulseAnimationWithDone(trs[i], () => done[localI] = true));
            }

            while (!done.All(x => x))
                yield return null;

            if (delayBetweenGroups > 0f)
                yield return new WaitForSeconds(delayBetweenGroups);
        }
    }

    private IEnumerator PulseAnimationWithDone(Transform tr, Action onDone)
    {
        yield return StartCoroutine(PulseAnimation(tr));
        onDone?.Invoke();
    }

    private IEnumerator PulseAnimation(Transform tr)
    {
        float duration = 0.5f;
        Vector3 original = tr.localScale;
        Vector3 target = original * 1.3f;

        float t = 0f;
        while (t < duration / 2f)
        {
            tr.localScale = Vector3.Lerp(original, target, t / (duration / 2f));
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < duration / 2f)
        {
            tr.localScale = Vector3.Lerp(target, original, t / (duration / 2f));
            t += Time.deltaTime;
            yield return null;
        }

        tr.localScale = original;
    }

    void GenerateResult()
    {
        var weights = new List<int>();

        for (int i = 0; i < symbols.Count; i++)
        {
            float luck = AIDifficultySystem.Instance ? AIDifficultySystem.Instance.playerLuck : 0f;
            float symbolBonus = GameManager.Instance ? GameManager.Instance.GetSymbolBaseBonus(symbols[i]) : 0f;

            float mod = luck + symbolBonus;
            weights.Add(Mathf.Max(1, Mathf.RoundToInt(symbols[i].weight * (1f + mod))));
        }

        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 5; c++)
                result[r, c] = PickWeighted(symbols, weights);
    }

    private Symbol PickWeighted(List<Symbol> pool, List<int> w)
    {
        int total = w.Sum();
        int roll = UnityEngine.Random.Range(1, total + 1);
        int sum = 0;

        for (int i = 0; i < pool.Count; i++)
        {
            sum += w[i];
            if (roll <= sum) return pool[i];
        }

        return pool[^1];
    }

    private int EvaluatePayout()
    {
        int total = 0;
        lastWinningGroups.Clear();

        foreach (var lineInfo in Payline.AllRowAndDiagonalLines)
        {
            int[] line = lineInfo.Pattern;

            int bestPayout = 0;
            int bestStartCol = -1;
            int bestCount = 0;
            Symbol bestSym = null;

            for (int startCol = 0; startCol <= 2; startCol++)
            {
                Symbol sym = result[line[startCol], startCol];
                int count = 1;

                for (int c = startCol + 1; c < 5; c++)
                {
                    if (result[line[c], c] == sym) count++;
                    else break;
                }

                if (count >= 3)
                {
                    int basePay = count == 3 ? sym.pay3 :
                                  count == 4 ? sym.pay4 : sym.pay5;

                    int finalPay = Mathf.RoundToInt(basePay * lineInfo.Multiplier);

                    if (finalPay > bestPayout)
                    {
                        bestPayout = finalPay;
                        bestStartCol = startCol;
                        bestCount = count;
                        bestSym = sym;
                    }
                }
            }

            if (bestPayout > 0)
            {
                total += bestPayout;

                var cellsGroup = new List<(int r, int c)>();
                for (int c = bestStartCol; c < bestStartCol + bestCount; c++)
                    cellsGroup.Add((line[c], c));

                lastWinningGroups.Add(new WinningGroup
                {
                    cells = cellsGroup,
                    payout = bestPayout
                });
            }
        }

        foreach (int col in Payline.Columns)
        {
            Symbol sym = result[0, col];
            if (result[1, col] == sym && result[2, col] == sym)
            {
                total += sym.pay3;

                lastWinningGroups.Add(new WinningGroup
                {
                    cells = new List<(int r, int c)> { (0, col), (1, col), (2, col) },
                    payout = sym.pay3
                });
            }
        }

        if (AIDifficultySystem.Instance)
            AIDifficultySystem.Instance.AdjustAfterSpin(total > 0);

        return total;
    }
}
