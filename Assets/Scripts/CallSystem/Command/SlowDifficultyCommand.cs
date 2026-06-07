using UnityEngine;
public class SlowDifficultyCommand : ICallCommand
{
    private AIDifficultySystem ai;

    public string Name => "Slow Difficulty";

    public SlowDifficultyCommand(AIDifficultySystem aiSystem)
    {
        ai = aiSystem;
    }

    public void Execute()
    {
        // Hacer el juego un poco más difícil reduciendo suerte y subiendo dificultad
        ai.BoostLuck(-0.10f);     
        ai.MakeHarder(0.10f);     

    }
}
