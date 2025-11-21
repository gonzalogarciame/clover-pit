using UnityEngine;
public interface ICallCommand
{
    string Name { get; }
    void Execute();
}