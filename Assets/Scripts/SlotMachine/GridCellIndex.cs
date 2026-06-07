using UnityEngine;

public sealed class GridCellIndex : MonoBehaviour
{
    [Range(0, 2)] public int row;
    [Range(0, 4)] public int col;
}
