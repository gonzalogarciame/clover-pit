using UnityEngine;

[CreateAssetMenu(menuName = "CloverPit/Symbol")]
public class Symbol : ScriptableObject
{
    public string id;
    public Sprite sprite;

    [Range(1, 100)] public int weight   = 10;
    public int pay3 = 10;
    public int pay4 = 30;
    public int pay5 = 100;
}

