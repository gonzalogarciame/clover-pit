using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "CloverPit/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public int price;
    public Sprite icon;
    public ItemEffectType effectType;
    public float effectValue;
    public float effectDuration;
    public Symbol targetSymbol;
}

public enum ItemEffectType
{
    IncreaseLuck,
    ReduceDifficulty,
    DoubleWins,
    FreeSpins,
    GuaranteedWin,
    UpgradePay3,
    UpgradePay4,
    UpgradePay5,
    IncreaseSymbolBaseChance


}
