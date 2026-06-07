using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public sealed class DifficultyMLAgent : Agent
{
    [SerializeField] private DifficultyDirector director;

    [Header("Episode")]
    [SerializeField] private int maxStepsPerEpisode = 300;
    private int steps;

    public override void Initialize()
    {
        if (!director) director = DifficultyDirector.Instance;
        steps = 0;
    }

    public override void OnEpisodeBegin()
    {
        steps = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!director || !director.IsReady)
        {
            for (int i = 0; i < 8; i++) sensor.AddObservation(0f);
            return;
        }

        director.GetObservations(out float bankrollNorm,
                                 out float winStreakNorm,
                                 out float lossStreakNorm,
                                 out float sessionNorm,
                                 out float freeSpinsNorm,
                                 out float recentPayoutMeanNorm,
                                 out float difficulty01,
                                 out float luckSigned);

        sensor.AddObservation(bankrollNorm);
        sensor.AddObservation(winStreakNorm);
        sensor.AddObservation(lossStreakNorm);
        sensor.AddObservation(sessionNorm);
        sensor.AddObservation(freeSpinsNorm);
        sensor.AddObservation(recentPayoutMeanNorm);
        sensor.AddObservation(difficulty01);
        sensor.AddObservation(luckSigned);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!director || !director.IsReady)
        {
            AddReward(-0.01f);
            return;
        }

        if (!director.ShouldAgentDecideNow())
        {
            AddReward(0f);
            return;
        }

        int difficultyDeltaAction = actions.DiscreteActions[0]; // 0,1,2
        int luckDeltaAction = actions.DiscreteActions[1]; // 0,1,2
        int debtTuningAction = actions.DiscreteActions[2]; // 0,1,2
        int payoutTuningAction = actions.DiscreteActions[3]; // 0,1,2
        int itemAction = actions.DiscreteActions[4]; // 0..3
        int protectionAction = actions.DiscreteActions[5]; // 0..1

        director.ApplyDecision(difficultyDeltaAction, luckDeltaAction, debtTuningAction, payoutTuningAction, itemAction, protectionAction);

        AddReward(director.ConsumeLastStepReward());

        steps++;
        if (maxStepsPerEpisode > 0 && steps >= maxStepsPerEpisode)
            EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var d = actionsOut.DiscreteActions;
        d[0] = 1;
        d[1] = 1;
        d[2] = 1;
        d[3] = 1;
        d[4] = 0;
        d[5] = 0;
    }
}
