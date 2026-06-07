using System;
using System.Collections.Generic;
using System.Linq;

public static class Payline
{
    public class PaylineInfo
    {
        public int[] Pattern { get; set; }
        public string Name { get; set; }
        public float Multiplier { get; set; } = 1.0f;
    }

    // FILAS 
    public static readonly List<PaylineInfo> Rows = new()
    {
        new PaylineInfo { Pattern = new[] { 0,0,0,0,0 }, Name = "Fila Superior",  Multiplier = 1.0f },
        new PaylineInfo { Pattern = new[] { 1,1,1,1,1 }, Name = "Fila Media",    Multiplier = 1.0f },
        new PaylineInfo { Pattern = new[] { 2,2,2,2,2 }, Name = "Fila Inferior", Multiplier = 1.0f },
    };

    // DIAGONALES
    public static readonly List<PaylineInfo> Diagonals = new()
    {
        new PaylineInfo { Pattern = new[] { 0,1,2,1,0 }, Name = "Diagonal V",           Multiplier = 1.2f },
        new PaylineInfo { Pattern = new[] { 2,1,0,1,2 }, Name = "Diagonal V Invertida", Multiplier = 1.2f },
    };

    // COLUMNAS 
    public static readonly List<int> Columns = new() { 0, 1, 2, 3, 4 };

    // Todas las líneas válidas
    public static IEnumerable<PaylineInfo> AllRowAndDiagonalLines => _allLines;
    public static IEnumerable<int[]> Lines => _allLines.Select(l => l.Pattern);

    private static readonly List<PaylineInfo> _allLines;

    static Payline()
    {
        var merged = Rows.Concat(Diagonals).ToList();

        foreach (var l in merged)
        {
            if (l.Pattern == null || l.Pattern.Length != 5)
                throw new Exception($"Payline inválida (debe tener 5 columnas): {l?.Name}");

            if (l.Pattern.Any(r => r < 0 || r > 2))
                throw new Exception($"Payline inválida (fila fuera de rango 0..2): {l.Name}");
        }

        _allLines = merged
            .GroupBy(l => string.Join(",", l.Pattern))
            .Select(g => g.First())
            .ToList();
    }
}
