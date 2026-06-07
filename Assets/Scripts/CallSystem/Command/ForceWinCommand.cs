using UnityEngine;

public class ForceWinCommand : ICallCommand
{
    private AIDifficultySystem ai;

    public string Name => "Force Win";

    public ForceWinCommand(AIDifficultySystem aiSystem)
    {
        ai = aiSystem;
    }

    public void Execute()
    {
        ai.ForceNextWin();
        Debug.Log("[Call] ForceWinCommand ejecutado: próximo spin será victoria");
    }
}
