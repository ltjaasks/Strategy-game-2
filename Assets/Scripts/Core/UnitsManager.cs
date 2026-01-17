using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class UnitsManager : MonoBehaviour
{
    public static UnitsManager Instance;
    private Dictionary<Vector2Int, List<Unit>> unitsByTile = new Dictionary<Vector2Int, List<Unit>>();
    private Dictionary<Unit, Vector2Int> NextMoves = new Dictionary<Unit, Vector2Int>();

    private List<Unit> allUnits = new List<Unit>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Debug.Log("UnitsManager initialized");
    }


    // Registers a unit with the UnitsManager on creation.
    public void RegisterUnit(Unit unit)
    {
        allUnits.Add(unit);

        if (!unitsByTile.TryGetValue(unit.GridPos, out var list)) // Check if there's already a list for this tile
        {
            list = new List<Unit>(); // Create a new empty list
            unitsByTile[unit.GridPos] = list; // Add the new list to the dictionary
        }
        list.Add(unit); // Add the unit to the list for its current tile

        // Subscribe to the unit's position change event to update its position in the manager on change.
        unit.OnGridPosChanged += (oldPos, newPos) => UpdateUnitPosition(unit, oldPos, newPos);

        Debug.Log("Unit registered in UnitsManager: " + unitsByTile.Count);
        OnUnitRegistered?.Invoke(unit);
    }

    public event Action<Unit> OnUnitRegistered;


    // Unregisters a unit from the UnitsManager on destruction.
    // Destruction happens on unit capture or removal from the game.
    // Called from CombatResolver or other game logic.
    public void UnRegisterUnit(Unit unit)
    {
        allUnits.Remove(unit);
        RemoveFromTile(unit, unit.GridPos);

        unit.OnGridPosChanged -= (oldPos, newPos) => UpdateUnitPosition(unit, oldPos, newPos);

        Destroy(unit.gameObject);
    }


    // Updates a unit's position in the unitsByTile dictionary when it moves.
    private void UpdateUnitPosition(Unit unit, Vector2Int oldPos, Vector2Int newPos)
    {
        RemoveFromTile(unit, oldPos);
        AddToTile(unit, newPos);
    }


    // Removes a unit from the specified tile in the unitsByTile dictionary.
    private void RemoveFromTile(Unit unit, Vector2Int position)
    {
        if (unitsByTile.TryGetValue(position, out var list)) // Check if there's a list for the tile
        {
            list.Remove(unit); // Remove the unit from the list

            if (list.Count == 0) // If the list is now empty, remove the entry from the dictionary
            {
                unitsByTile.Remove(position);
            }
        }
    }


    // Adds a unit to the specified tile in the unitsByTile dictionary.
    private void AddToTile(Unit unit, Vector2Int position)
    {
        if (!unitsByTile.TryGetValue(position, out var list)) // Check if there's already a list for this tile
        {
            list = new List<Unit>(); // Create a new empty list
            unitsByTile[position] = list; // Add the new list to the dictionary
        }
        list.Add(unit); // Add the unit to the list for its current tile
    }


    // Checks if a tile has same team units present. Allies block movement.
    // Returns true if an same team unit is found on the tile, false otherwise.
    public bool CheckTileForAllies(Vector2Int position, Boolean isAlly)
    {
        if (unitsByTile.TryGetValue(position, out var list))
        {
            Unit unit = list[0];
            return unit.IsAlly == isAlly;
        }

        return false;
    }


    // Checks all tiles for multiple units and triggers combat resolution if found.
    public void CheckForCombat()
    {
        foreach (var kvp in unitsByTile)
        {
            var unitsOnTile = kvp.Value;
            if (unitsOnTile.Count < 2) continue; // If key value pair has less than 2 units continue (no combat)

            CombatResolver.Instance.ResolveCombat(unitsOnTile);
        }
    }


    // Check for units trying to swap tiles and handle collisions. Collision of different teams triggers combat.
    public void CheckForCollisions()
    {
        var processedUnits = new HashSet<Unit>();
        foreach (var kvp in NextMoves)
        {
            Unit unitA = kvp.Key;
            Vector2Int targetPosA = kvp.Value;
            if (processedUnits.Contains(unitA))
                continue;
            foreach (var kvp2 in NextMoves)
            {
                Unit unitB = kvp2.Key;
                Vector2Int targetPosB = kvp2.Value;
                if (unitA == unitB || processedUnits.Contains(unitB))
                    continue;
                // Check for swap
                if (unitA.IsAlly == unitB.IsAlly)
                {
                    //unitA.CancelMove();
                    //unitB.CancelMove();
                    continue;
                }
                if (unitA.GridPos == targetPosB && unitB.GridPos == targetPosA)
                {
                    CombatResolver.Instance.ResolveCollision(unitA, unitB);
                    break;
                }
            }
        }
    }


    // Collects the next intended moves for all units in NextMoves dictionary.
    public void CollectNextMoves()
    {
        NextMoves.Clear();
        foreach (var unit in allUnits)
        {
            Vector2Int? temp = unit.GetNextTileInPath();

            if (temp == null)
            {
                continue; // Skip units with no next tile
            }

            Vector2Int next = temp.Value;

            NextMoves[unit] = next;
        }
    }


    // Checks which side controls a given tile based on unit influence.
    // Returns 1 if controlled by allies, -1 if controlled by enemies, 0 if neutral.
    public List<Unit> CheckControlOfTile(Vector2Int tile)
    {
        List <Unit> controllingUnits = new List<Unit>();

        foreach (var unit in allUnits)
        {
            if (unit.GetControlledTiles(unit.GridPos).Contains(tile))
            {
                controllingUnits.Add(unit);
            }
        }

        return controllingUnits;
    }

    public Unit GetUnitAt(Vector2Int position)
    {
        foreach (var unit in allUnits)
        {
            Debug.Log($"Checking unit at {unit.GridPos} against position {position}");
            if (unit.GridPos == position)
            {
                return unit;
            }
        }
        return null;
    }


    public Unit[] GetUnitsByFaction(bool isAlly)
    {
        var unitsByFaction = new List<Unit>();
        foreach (var unit in allUnits)
        {
            if (unit.IsAlly == isAlly)
            {
                unitsByFaction.Add(unit);
            }
        }

        return unitsByFaction.ToArray();
    }
}
