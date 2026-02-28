using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Class to manage all units collectively in the game.
/// </summary>
public class UnitsManager : MonoBehaviour
{
    public static UnitsManager Instance;

    // Dictionary mapping tile positions to the unit currently occupying that tile.
    private Dictionary<Vector2Int, Unit> UnitsByTile = new Dictionary<Vector2Int, Unit>();

    // List of all units in the game for easy iteration and management.
    private List<Unit> _allUnits = new List<Unit>();

    // Dictionary to store event handlers for unit position changes, allowing us to unsubscribe when units are unregistered.
    private Dictionary<Unit, Action<Vector2Int, Vector2Int>> _posChangedHandlers = new Dictionary<Unit, Action<Vector2Int, Vector2Int>>();


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
    /// Registers a unit with the UnitsManager on creation.
    /// </summary>
    /// <param name="unit"></param>
    public void RegisterUnit(Unit unit)
    {
        _allUnits.Add(unit);

        if (UnitsByTile.TryGetValue(unit.GridPos, out var existing)) // Check if there's already a list for this tile
        {
            Debug.LogWarning("Tile already has unit");
        }
        UnitsByTile[unit.GridPos] = unit; // Add the new list to the dictionary

        // Subscribe to the unit's position change event to update its position in the manager on change.
        Action<Vector2Int, Vector2Int> handler = (oldPos, newPos) => UpdateUnitPosition(unit, oldPos, newPos);
        unit.OnGridPosChanged += handler;
        _posChangedHandlers[unit] = handler;

        OnUnitRegistered?.Invoke(unit);
    }

    public event Action<Unit> OnUnitRegistered;


    /// <summary>
    /// Unregisters a unit from UnitsManager on destruction
    /// </summary>
    /// <param name="unit">Unit to destroy</param>
    public void UnRegisterUnit(Unit unit)
    {
        _allUnits.Remove(unit);
        RemoveFromTile(unit, unit.GridPos);

        // Unsubscribe from the unit's position change event.
        if (_posChangedHandlers.TryGetValue(unit, out var handler))
        {
            unit.OnGridPosChanged -= handler;
            _posChangedHandlers.Remove(unit);
        }

        Destroy(unit.gameObject);
    }


    /// <summary>
    /// Updates a unit's position in the unitsByTile dictionary when it moves.
    /// </summary>
    /// <param name="unit">Unit to update</param>
    /// <param name="oldPos">Old position of unit</param>
    /// <param name="newPos">New Position of unit</param>
    private void UpdateUnitPosition(Unit unit, Vector2Int oldPos, Vector2Int newPos)
    {
        RemoveFromTile(unit, oldPos);
        AddToTile(unit, newPos);
    }


    /// <summary>
    /// Removes a unit from the specified tile in the unitsByTile dictionary.
    /// </summary>
    /// <param name="unit">Unit to remove</param>
    /// <param name="position">Position the unit was on</param>
    private void RemoveFromTile(Unit unit, Vector2Int position)
    {
        if (UnitsByTile.TryGetValue(position, out var existing)) // Check if there's an entry for the tile
        {
            // Only remove if the stored unit matches the one being removed
            if (existing == unit)
            {
                UnitsByTile.Remove(position);
            }
            else
            {
                // If existing is not the same unit, we leave it (this can happen if you replaced the tile entry earlier)
                Debug.Log($"RemoveFromTile: tile {position} contains different unit ({existing?.name}), not removing.");
            }
        }
    }


    /// <summary>
    /// Adds a unit to the specified tile in the unitsByTile dictionary.
    /// </summary>
    /// <param name="unit">Unit to add</param>
    /// <param name="position">Position to add the unit</param>
    private void AddToTile(Unit unit, Vector2Int position)
    {
        if (UnitsByTile.TryGetValue(position, out var existing))
        {
            Debug.LogWarning($"AddToTile: tile {position} already occupied by {existing?.name}. Replacing with {unit.name}.");
        }
        UnitsByTile[position] = unit;
    }


    /// <summary>
    /// Checks if a tile has enemy team units.
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="isAlly"></param>
    /// <returns>Returns true if tile has enemy, otherwise returns false</returns>
    public bool CheckTileForEnemy(Vector2Int tile, Boolean isAlly)
    {
        if (UnitsByTile.TryGetValue(tile, out var unit))
        {
            return unit.IsAlly != isAlly;
        }

        return false;
    }


    /// <summary>
    /// Get unit at specific position.
    /// </summary>
    /// <param name="position">Position to check for unit</param>
    /// <returns>Unit found or null if no unit was found</returns>
    public Unit GetUnitAt(Vector2Int position)
    {
        foreach (var unit in _allUnits)
        {
            if (unit.GridPos == position)
            {
                return unit;
            }
        }
        return null;
    }


    /// <summary>
    /// Returns an array of units that are either allies or enemies based on the isAlly parameter.
    /// </summary>
    /// <param name="isAlly">The team of which units to get</param>
    /// <returns>Array of units of specific team</returns>
    public Unit[] GetUnitsByFaction(bool isAlly)
    {
        var unitsByFaction = new List<Unit>();
        foreach (var unit in _allUnits)
        {
            if (unit.IsAlly == isAlly)
            {
                unitsByFaction.Add(unit);
            }
        }

        return unitsByFaction.ToArray();
    }


    /// <summary>
    /// Get the dictionary mapping tile positions to the unit currently occupying that tile.
    /// </summary>
    /// <returns>Dictionary mapping tile positions to units</returns>
    public Dictionary<Vector2Int, Unit> GetUnitsByTile()
    {
        return UnitsByTile;
    }


    public Unit[] GetUnits => _allUnits.ToArray();

}
