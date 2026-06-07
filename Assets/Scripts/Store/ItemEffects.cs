using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemEffects : MonoBehaviour
{
    public static ItemEffects Instance { get; private set; }

    private readonly Dictionary<ItemEffectType, Coroutine> running = new();

    private int spinSignal = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnSpinFinished += HandleSpinFinished;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnSpinFinished -= HandleSpinFinished;
    }

    private void HandleSpinFinished()
    {
        spinSignal++;
    }

    public void ApplyItemEffect(Item item)
    {
        // Solo cancelamos el mismo tipo (si compras el mismo buff otra vez)
        if (running.TryGetValue(item.effectType, out var co) && co != null)
        {
            StopCoroutine(co);
            running.Remove(item.effectType);
        }

        var newCo = StartCoroutine(ApplyEffectRoutine(item));
        running[item.effectType] = newCo;
    }

    private IEnumerator ApplyEffectRoutine(Item item)
    {
        switch (item.effectType)
        {
            case ItemEffectType.IncreaseLuck:
                yield return StartCoroutine(IncreaseLuckEffect(item));
                break;

            case ItemEffectType.ReduceDifficulty:
                yield return StartCoroutine(ReduceDifficultyEffect(item));
                break;

            case ItemEffectType.DoubleWins:
                yield return StartCoroutine(DoubleWinsEffect(item));
                break;

            case ItemEffectType.FreeSpins:
                yield return StartCoroutine(FreeSpinsEffect(item));
                break;

            case ItemEffectType.GuaranteedWin:
                yield return StartCoroutine(GuaranteedWinEffect(item));
                break;

            case ItemEffectType.UpgradePay3:
                GameManager.Instance.Upgrade3(item.effectValue);
                UIManager.Instance.ShowEffectMessage($"+{item.effectValue:P0} a pagos de 3");
                break;

            case ItemEffectType.UpgradePay4:
                GameManager.Instance.Upgrade4(item.effectValue);
                UIManager.Instance.ShowEffectMessage($"+{item.effectValue:P0} a pagos de 4");
                break;

            case ItemEffectType.UpgradePay5:
                GameManager.Instance.Upgrade5(item.effectValue);
                UIManager.Instance.ShowEffectMessage($"+{item.effectValue:P0} a pagos de 5");
                break;
            case ItemEffectType.IncreaseSymbolBaseChance:
                ApplyIncreaseSymbolBaseChance(item);
                break;

        }

        running.Remove(item.effectType);
    }

    private IEnumerator WaitSpins(int spins)
    {
        spins = Mathf.Max(0, spins);
        for (int i = 0; i < spins; i++)
        {
            int start = spinSignal;
            yield return new WaitUntil(() => spinSignal > start);
        }
    }


    private IEnumerator IncreaseLuckEffect(Item item)
    {
        float originalLuck = AIDifficultySystem.Instance.playerLuck;
        AIDifficultySystem.Instance.playerLuck += item.effectValue;

        UIManager.Instance.ShowEffectMessage($"¡Suerte aumentada! +{item.effectValue} (x{(int)item.effectDuration} giros)");
        yield return WaitSpins((int)item.effectDuration);

        AIDifficultySystem.Instance.playerLuck = originalLuck;
        UIManager.Instance.ShowEffectMessage("Efecto de suerte terminado");
    }

    private IEnumerator ReduceDifficultyEffect(Item item)
    {
        float originalDifficulty = AIDifficultySystem.Instance.difficulty;
        AIDifficultySystem.Instance.difficulty -= item.effectValue;

        UIManager.Instance.ShowEffectMessage($"¡Dificultad reducida! -{item.effectValue} (x{(int)item.effectDuration} giros)");
        yield return WaitSpins((int)item.effectDuration);

        AIDifficultySystem.Instance.difficulty = originalDifficulty;
        UIManager.Instance.ShowEffectMessage("Efecto de dificultad terminado");
    }

    private IEnumerator DoubleWinsEffect(Item item)
    {
        GameManager.Instance.IsDoubleWinsActive = true;
        UIManager.Instance.ShowEffectMessage($"¡Ganancias dobles! (x{(int)item.effectDuration} giros)");

        yield return WaitSpins((int)item.effectDuration);

        GameManager.Instance.IsDoubleWinsActive = false;
        UIManager.Instance.ShowEffectMessage("Ganancias dobles terminadas");
    }

    private IEnumerator FreeSpinsEffect(Item item)
    {
        int freeSpins = Mathf.RoundToInt(item.effectValue);
        GameManager.Instance.AddFreeSpins(freeSpins);
        UIManager.Instance.ShowEffectMessage($"¡{freeSpins} giros gratis!");
        yield break;
    }

    private IEnumerator GuaranteedWinEffect(Item item)
    {
        GameManager.Instance.IsGuaranteedWinActive = true;
        UIManager.Instance.ShowEffectMessage("¡Victoria garantizada en el próximo giro!");
        yield return new WaitUntil(() => !GameManager.Instance.IsGuaranteedWinActive);
    }

    private void ApplyIncreaseSymbolBaseChance(Item item)
    {
        if (item.targetSymbol == null)
        {
            Debug.LogWarning("Item sin targetSymbol");
            return;
        }

        GameManager.Instance.AddSymbolBaseBonus(
            item.targetSymbol,
            0.20f   
        );

        UIManager.Instance.ShowEffectMessage(
            $"✨ {item.targetSymbol.id}: +20% probabilidad (ACUMULADO)"
        );
    }

}
