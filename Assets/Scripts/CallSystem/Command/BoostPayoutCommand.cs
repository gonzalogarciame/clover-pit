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

        ai.BoostLuck(0.15f);      
        ai.MakeEasier(0.10f);     

    }
}
