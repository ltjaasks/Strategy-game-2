using System.Threading;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    public int tickRate = 1;
    private float timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating(nameof(Tick), tickRate, tickRate);
    }

    void Tick()
    {
        // Debug.Log("Tick");
        // Move units each tick
        foreach (Unit unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            UnitsManager.Instance.CollectNextMoves();
            UnitsManager.Instance.CheckForCollisions();
            unit.BeginTick();
            
            unit.Move();
        }
        //UnitsManager.Instance.CheckForCollisions();
        UnitsManager.Instance.CheckForCombat();
    }
}
