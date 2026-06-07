using System.Collections;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    public bool IsSpinning { get; private set; }

    public void Spin()
    {
        if (IsSpinning) return;
        StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        IsSpinning = true;
        yield return new WaitForSeconds(1.2f);

        // IA decide si gana o pierde
        bool win = AIDifficultySystem.Instance.GetSpinResult();
        int amount = win ? Random.Range(5, 31) : -10;

        GameManager.Instance.AddMoney(amount);

        // Ajustar la IA según el resultado
        AIDifficultySystem.Instance.AdjustAfterSpin(win);

        IsSpinning = false;
    }

}
