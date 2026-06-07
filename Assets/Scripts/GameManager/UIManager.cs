
using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Texts")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text freeSpinsText;
    [SerializeField] private TMP_Text roundInfoText;

    [Header("Popups / Messages")]
    [SerializeField] private TMP_Text effectMessageText;
    [SerializeField] private TMP_Text resultPopupText;

    private Coroutine effectCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RefreshMoney(int amount)
    {
        if (moneyText) moneyText.text = $"$ {amount}";
    }

    public void RefreshFreeSpins(int freeSpins)
    {
        if (freeSpinsText) freeSpinsText.text = $"Free Spins: {freeSpins}";
    }

    public void RefreshRoundUI(int debtMultiplier, int spinsLeft, int spinCost, int goalMoney)
    {
        if (roundInfoText)
        {
            roundInfoText.text =
                $"Deuda x{debtMultiplier} | Tiradas: {spinsLeft} | Coste: ${spinCost} | Objetivo: ${goalMoney}";
        }
    }

    public void ShowEffectMessage(string message, float duration = 1.5f)
    {
        if (!effectMessageText) return;

        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);

        effectCoroutine = StartCoroutine(EffectMessageRoutine(message, duration));
    }

    private IEnumerator EffectMessageRoutine(string message, float duration)
    {
        effectMessageText.gameObject.SetActive(true);
        effectMessageText.text = message;

        yield return new WaitForSeconds(duration);

        effectMessageText.gameObject.SetActive(false);
    }

    public void ShowResultPopup(int amount)
    {
        if (!resultPopupText) return;

        CancelInvoke(nameof(HideResultPopup));

        resultPopupText.gameObject.SetActive(true);
        resultPopupText.text = amount > 0 ? $"+{amount}" : "NO WIN";

        Invoke(nameof(HideResultPopup), 1.2f);
    }

    private void HideResultPopup()
    {
        if (resultPopupText)
            resultPopupText.gameObject.SetActive(false);
    }
}
