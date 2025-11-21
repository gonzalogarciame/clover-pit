using UnityEngine;
public class BoostPayoutCommand : ICallCommand
{
    private AIDifficultySystem ai;

    public string Name => "Boost Payout";

    public BoostPayoutCommand(AIDifficultySystem aiSystem)
    {
        ai = aiSystem;
    }

    public void Execute()
    {
        // Subimos bastante la suerte y hacemos el juego algo más fácil
        ai.BoostLuck(0.15f);      // +15% de suerte
        ai.MakeEasier(0.10f);     // -10% de dificultad

        Debug.Log("[Call] BoostPayoutCommand ejecutado: suerte y facilidad aumentadas");
    }
}
