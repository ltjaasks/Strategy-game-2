using Game;
using UnityEngine;


/// <summary>
/// Manages the game ticks and order of operations during a game tick.
/// Can operate in automatic mode (ticks advance based on tickRate) or
/// manual mode (ticks advance when ManualTick is called).
/// </summary>
public class TickManager : MonoBehaviour
{
    public static TickManager Instance;

    [Header("Tick Settings")]
    public bool automaticTicks = true;   // Toggle to swap between automatic and manual ticking
    public float tickRate = 0.5f;          // ticks per second

    private float timer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        if (!automaticTicks) // skip ticking in Update if in manual mode
            return;

        if (GameManager.Instance.State != GameState.Playing)
            return;

        timer += Time.deltaTime;

        float tickInterval = 1f / tickRate;

        // Calls Tick according to timer
        while (timer >= tickInterval)
        {
            timer -= tickInterval;
            Tick();
        }
    }


    /// <summary>
    /// Manual tick call.
    /// </summary>
    public void ManualTick()
    {
        if (automaticTicks)
            return; // prevents accidental double ticks

        Tick();
    }


    /// <summary>
    /// Runs a game tick. Contains order of operations during tick to maintain game logic.
    /// </summary>
    void Tick()
    {
        if (GameManager.Instance.State != GameState.Playing)
            return;

        TickResolver.Instance.ResolveTick();
        EnemyController.Instance.MoveEnemies();

        VisionManager.Instance.UpdateVision();
        FogController.Instance.UpdateFog();
    }
}
