using UnityEngine;

using System;
using System.Collections.Generic;

public static class CallSystem
{
    // Observer → otros sistemas escuchan cuando se dispara una call
    public static event EventHandler<CallEventArgs> OnCallExecuted;

    // Cola opcional: si alguna vez quieres acumular calls pendientes
    private static Queue<ICallCommand> callQueue = new Queue<ICallCommand>();

    // Envia una call inmediatamente
    public static void ExecuteCall(ICallCommand command)
    {
        command.Execute();
        OnCallExecuted?.Invoke(null, new CallEventArgs(command));
    }

    // Envia una call más adelante (por ejemplo al final de un spin)
    public static void EnqueueCall(ICallCommand command)
    {
        callQueue.Enqueue(command);
    }

    // Procesa todas las calls en cola
    public static void ProcessQueue()
    {
        while (callQueue.Count > 0)
        {
            var cmd = callQueue.Dequeue();
            cmd.Execute();
            OnCallExecuted?.Invoke(null, new CallEventArgs(cmd));
        }
    }
}

public class CallEventArgs : EventArgs
{
    public ICallCommand Command { get; }

    public CallEventArgs(ICallCommand command)
    {
        Command = command;
    }
}

