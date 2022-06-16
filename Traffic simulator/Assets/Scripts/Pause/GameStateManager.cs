using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum GameState { Play, Pause }

public class GameStateManager : MonoBehaviour
{
    public static GameState CurrentGameState { get; private set; }

    public static UnityEvent<GameState> OnGameStateChanged = new UnityEvent<GameState>();

    private void Start()
    {
        CurrentGameState = GameState.Pause;
    }

    static void SetState(GameState newGameState)
    {
        if (newGameState == CurrentGameState)
            return;

        CurrentGameState = newGameState;
        OnGameStateChanged?.Invoke(newGameState);
    }

    public static void Play()
    {
        SetState(GameState.Play);
    }

    public static void Pause()
    {
        SetState(GameState.Pause);
    }

    public static void Stop()
    {
        foreach(IPauseable pauseable in FindObjectsOfType<MonoBehaviour>().OfType<IPauseable>())
        {
            pauseable.OnRestart();
        }

        foreach (CrossroadPath crossroadPath in FindObjectsOfType<CrossroadPath>())
        {
            crossroadPath.ClearCarsByLanes();
        }

        SetState(GameState.Pause);
    }
}
