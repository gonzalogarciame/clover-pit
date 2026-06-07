using UnityEngine;

public class ForceLossCommand : ICallCommand
{
    private AIDifficultySystem ai;

    public string Name => "Force Loss";

    public ForceLossCommand(AIDifficultySystem aiSystem)
    {
        ai = aiSystem;
    }

    public void Execute()
    {
        ai.ForceNextLoss();
        Debug.Log("[Call] ForceLossCommand ejecutado: próximo spin será derrota");
    }
}

