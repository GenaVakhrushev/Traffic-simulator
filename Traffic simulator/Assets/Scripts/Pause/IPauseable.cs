using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPauseable
{
    public void OnRestart();
    void OnGameStateChanged(GameState gameState);
}
