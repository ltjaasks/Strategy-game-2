using Game;
using UnityEngine;


/// <summary>
/// Manages game state and win conditions.
/// </summary>
public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    public GameState State { get; private set; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        State = GameState.Playing;
    }

    /// <summary>
    /// Ends game when win conditions are met. Win condition is commander killed.
    /// </summary>
    /// <param name="playerWon"></param>
    public void EndGame(bool playerWon)
    {
        if (State == GameState.GameOver)
            return;

        State = GameState.GameOver;

        Debug.Log(playerWon ? "PLAYER WINS" : "ENEMY WINS");
    }
}
