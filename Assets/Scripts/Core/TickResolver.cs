using Actions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Units;
using UnityEngine;


/// <summary>
/// Class that resolves move intentions for all units each tick and applies the results.
/// </summary>
public class TickResolver : MonoBehaviour
{
    public static TickResolver Instance;

    // Stores units intended next moves
    Dictionary<Vector2Int, List<Unit>> NextMoves = new Dictionary<Vector2Int, List<Unit>>(); // Intended board state next tick

    // Current board state
    Dictionary<Vector2Int, Unit> CurrentState;

    // Final resolved move results for each unit after resolution process
    Dictionary<Unit, MoveResult> ResolvedMoves;

    // Tick counter for debugging and logging purposes
    private int TickCount = 0;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    /// <summary>
    /// Main method and controller of movement resolution flow.
    /// </summary>
    public void ResolveTick()
    {
        TickCount++;

        // Get current state and clear previous data to prepare for tick resolution
        Unit[] units = UnitsManager.Instance.GetUnits;
        CurrentState = new Dictionary<Vector2Int, Unit>(UnitsManager.Instance.GetUnitsByTile());
        NextMoves.Clear();

        // Collect intended moves of all units for next tick
        CollectNextMoves(units);

        // Resolve conflicts and interactions based on intended moves and current board state
        ResolveMoves();

        Debug.Log($"=== Tick {TickCount} ===");

        // Debug log for testing purposes
        /*
        foreach (var kvp in ResolvedMoves)
        {
            if (kvp.Value == MoveResult.Dies)
                Debug.Log($"Unit at {kvp.Key.GridPos} -> {kvp.Key.GetNextTile()} {kvp.Value} {kvp.Key.MoveType}");
        }
        */

        // Apply movement results to units.
        ApplyResults();
    }


    /// <summary>
    /// Collects the next intended moves of all units into a dictionary
    /// mapping target positions to lists of units intending to move there
    /// </summary>
    /// <param name="units"></param>
    public void CollectNextMoves(Unit[] units)
    {
        foreach (Unit unit in units)
        {
            Vector2Int nextStep = unit.GetNextTile();

            if (!NextMoves.ContainsKey(nextStep))
            {
                NextMoves[nextStep] = new List<Unit>();
            }
            NextMoves[nextStep].Add(unit);
        }
    }


    /// <summary>
    /// Helper method to structure resolution process.
    /// </summary>
    public void ResolveMoves()
    {
        var results = InitializeResults();

        LoopUntilStable(results);

        ResolvedMoves = results;
    }


    /// <summary>
    /// Initializes the results dictionary for move resolution
    /// Stationary units are immediately allowed, rest are set to unresolved
    /// </summary>
    /// <returns>The initialized results dictionary.</returns>
    public Dictionary<Unit, MoveResult> InitializeResults()
    {
        var results = new Dictionary<Unit, MoveResult>();

        foreach (var tile in NextMoves.Values)
        {
            foreach (var unit in tile)
            {
                if (unit.MoveType == ActionType.Wait)
                {
                    results[unit] = MoveResult.Allowed;
                    continue;
                }
                else
                    results[unit] = MoveResult.Unresolved;
            }
        }

        return results;
    }


    /// <summary>
    /// Loops through the resolution process until no more changes occur.
    /// </summary>
    /// <param name="results">Results dictionary to record the resolution process</param>
    public void LoopUntilStable(Dictionary<Unit, MoveResult> results)
    {
        bool changed; // Indicates if changes were made in the current pass, if not, resolution is complete
        int safety = 0; // Failsafe to prevent infinite loops, should never reach over unit count

        do
        {
            safety++;
            if (safety > CurrentState.Count * 2)
            {
                Debug.Log("Resolver infinite loop");
                break;
            }

            changed = ResolveOnePass(results);
        } while (changed);

        // Any unresolved moves at this point are allowed, as they have no conflicts or interactions
        foreach (var unit in results.Keys.ToList())
        {
            if (results[unit] == MoveResult.Unresolved)
                results[unit] = MoveResult.Allowed;
        }
    }


    /// <summary>
    /// One pass through the results dictionary, attempting to resolve moves based on current known information.
    /// </summary>
    /// <param name="results">Results dictionary to go through.</param>
    /// <returns>True if dictionary was changed, false if no changes made.</returns>
    public bool ResolveOnePass(Dictionary<Unit, MoveResult> results)
    {
        bool changed = false;

        foreach (var kvp in NextMoves)
        {
            var tile = kvp.Key;
            var contenders = GetUnresolvedContenders(kvp.Value, results);

            if (contenders.Count == 0)
                continue; // No unit moving here, continue to next tile

            changed = ResolveTile(tile, contenders, results);
        }
        return changed;
    }


    /// <summary>
    /// Gets the list of units intending to move to a tile that are still unresolved in the results dictionary.
    /// </summary>
    /// <param name="units">Units intending to move to the tile</param>
    /// <param name="results">The results dictionary containing units and moveresults</param>
    /// <returns></returns>
    public List<Unit> GetUnresolvedContenders(List<Unit> units, Dictionary<Unit, MoveResult> results)
    {
        List<Unit> contenders = new List<Unit>();

        foreach (Unit u in units)
        {
            if (results[u] == MoveResult.Unresolved)
            {
                contenders.Add(u);
            }
        }

        return contenders;
    }


    /// <summary>
    /// Resolves single tile for intending movers
    /// </summary>
    /// <param name="tile">Tile to resolve.</param>
    /// <param name="contenders">Units contending for the tile.</param>
    /// <param name="results">The results dictionary containing units and moveresults</param>
    /// <returns>True if results was changed, false if no changes made.</returns>
    public bool ResolveTile(Vector2Int tile, List<Unit> contenders, Dictionary<Unit, MoveResult> results)
    {
        switch (contenders.Count)
        {
            case 1:
                return ResolveSingleMover(tile, contenders[0], results);
            case 2:
                return ResolveTwoMovers(tile, contenders[0], contenders[1], results);
            default:
                return CancelAll(contenders, results);
        }
    }


    /// <summary>
    /// Resolves a tile with a single intending mover, checking for conflicts with current occupant and interactions like attacks.
    /// </summary>
    /// <param name="tile">Tile to resolve</param>
    /// <param name="unit">Unit intending to move to the tile</param>
    /// <param name="results">The results dictionary containing units and moveresults</param>
    /// <returns>True if results was changed, false if no changes made.</returns>
    public bool ResolveSingleMover(Vector2Int tile, Unit unit, Dictionary<Unit, MoveResult> results)
    {
        CurrentState.TryGetValue(tile, out var occupant);

        if (unit.MoveType == ActionType.Attack)
        {
            return ResolveAttack(tile, unit, results);
        }

        // Tile is empty, allow move
        if (occupant == null)
        {
            results[unit] = MoveResult.Allowed;
            return true;
        }

        if (occupant == unit)
            return false;

        // Swap case: two units moving into each other's tiles, cancel both moves
        if (occupant.GetNextTile() == unit.GridPos)
        {
            //Debug.Log("Swap case reached");
            Debug.Log("Unit " + unit.name + " at " + unit.GridPos + " and " + occupant.name + " at " + occupant.GridPos +  " trying to swap tiles, cancelling both moves");

            results[unit] = MoveResult.Cancelled;
            results[occupant] = MoveResult.Cancelled;
            return true;
        }

        if (results[occupant] == MoveResult.Allowed && occupant.MoveType != ActionType.Wait) // Occupant is moving out, allow move
        {
            results[unit] = MoveResult.Allowed;
            return true;
        }

        if (results[occupant] == MoveResult.Unresolved)
        {
            return false; // Not solved yet
        }

        results[unit] = MoveResult.Cancelled;
        return true;
    }


    /// <summary>
    /// Resolves an attack move.
    /// </summary>
    /// <param name="tile">Tile intended to attack to.</param>
    /// <param name="attacker">Unit attacking.</param>
    /// <param name="results">The results dictionary containing units and moveresults</param>
    /// <returns>True if results was changed, false if no changes made.</returns>
    public bool ResolveAttack(Vector2Int tile, Unit attacker, Dictionary<Unit, MoveResult> results)
    {
        Vector2Int attackTarget = (Vector2Int) attacker.AttackTarget;

        CurrentState.TryGetValue(attackTarget, out var occupant);

        // Attacking empty tile -> cancel move
        if (occupant == null)
        {
            results[attacker] = MoveResult.Cancelled;
            return true;
        }

        // TODO: Both attacking each other case

        // Occupant's move not resolved yet, wait
        if (results[occupant] == MoveResult.Unresolved)
        {
            return false; // Not solved yet
        }

        if (results[occupant] == MoveResult.Allowed && occupant.GetNextTile() != attackTarget)
        {
            results[attacker] = MoveResult.Cancelled;
            return true;
        }

        // Target stayed -> attacker wins
        results[attacker] = MoveResult.Allowed;
        results[occupant] = MoveResult.Dies;

        return true;
    }


    /// <summary>
    /// Resolve two units contending for the same tile,
    /// checking their move types and team affiliations to determine outcomes like cancellations, engagements or deaths.
    /// </summary>
    /// <param name="tile">Target tile</param>
    /// <param name="a">Unit a</param>
    /// <param name="b">Unit b</param>
    /// <param name="results">The results dictionary containing units and moveresults</param>
    /// <returns>True if results was changed, false if no changes made.</returns>
    public bool ResolveTwoMovers(Vector2Int tile, Unit a, Unit b, Dictionary<Unit, MoveResult> results)
    {
        // Two units trying to move to same tile without attack intention -> cancel both moves
        // Teams dont matter here, two units can't move to same tile without one attacking
        if (a.MoveType != ActionType.Attack && b.MoveType != ActionType.Attack)
        {
            results[a] = MoveResult.Cancelled;
            results[b] = MoveResult.Cancelled;
            return true;
        }

        // Allies trying to move to same tile -> cancel both moves
        // Failsafe, will only allow attacking to valid targets in movement controller later
        if (a.IsAlly == b.IsAlly)
        {
            results[a] = MoveResult.Cancelled;
            results[b] = MoveResult.Cancelled;
            return true;
        }

        // Both attacking -> engage
        if (a.MoveType == ActionType.Attack && b.MoveType == ActionType.Attack)
        {
            results[a] = MoveResult.Engages;
            results[b] = MoveResult.Engages;
            return true;
        }

        // One attacking
        if (a.MoveType == ActionType.Attack && b.MoveType != ActionType.Attack)
        {
            results[a] = MoveResult.Allowed;
            results[b] = MoveResult.Dies;
            return true;
        }
        if (a.MoveType != ActionType.Attack && b.MoveType == ActionType.Attack)
        {
            results[a] = MoveResult.Dies;
            results[b] = MoveResult.Allowed;
            return true;
        }
        return false; // Should never reach here
    }


    /// <summary>
    /// Cancels all moves for a list of units.
    /// </summary>
    /// <param name="units">List of units to cancel</param>
    /// <param name="results">The results dictionary containing units and moveresults</param>
    /// <returns>True because results was changed</returns>
    public bool CancelAll(List<Unit> units, Dictionary<Unit, MoveResult> results)
    {
        foreach (var u in units)
        {
            results[u] = MoveResult.Cancelled;
        }
        return true;
    }


    /// <summary>
    /// Applies the resolved move results to the units,
    /// calling appropriate methods on each unit based on their final MoveResult (Allowed, Cancelled, Engages, Dies).
    /// </summary>
    public void ApplyResults()
    {
        CurrentState.Clear();

        foreach (var kvp in ResolvedMoves)
        {
            Unit unit = kvp.Key;
            MoveResult result = kvp.Value;

            switch (result)
            {
                case MoveResult.Allowed:
                    unit.ApplyMove();
                    break;
                case MoveResult.Cancelled:
                    unit.ApplyCancel();
                    break;
                case MoveResult.Engages:
                    unit.ApplyEngage();
                    break;
                case MoveResult.Dies:
                    unit.ApplyDeath();
                    break;
            }
        }
    }
}
