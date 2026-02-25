using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class handles the logic of detecting visible tiles for each team.
/// </summary>
public class VisionManager : MonoBehaviour
{
    public static VisionManager Instance;

    public Dictionary<bool, HashSet<Vector2Int>> VisibleTiles = new Dictionary<bool, HashSet<Vector2Int>>()
    {
        { true, new HashSet<Vector2Int>() },  // Ally visible tiles
        { false, new HashSet<Vector2Int>() }  // Enemy visible tiles
    };

    public Unit[] Units;


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
    /// Updates the vision data each tick.
    /// </summary>
    public void UpdateVision()
    {
        Units = UnitsManager.Instance.GetUnits;

        // Clear previous vision data
        VisibleTiles[true].Clear();
        VisibleTiles[false].Clear();

        foreach (Unit unit in Units)
        {
            CalculateUnitVision(unit);
        }
    }


    /// <summary>
    /// Calculates units vision in Manhattan distance and updates the visible tiles for the unit's team.
    /// </summary>
    /// <param name="unit">Unit whose vision is calculated</param>
    public void CalculateUnitVision(Unit unit)
    {
        Vector2Int unitPos = unit.GridPos;

        for (int x = -unit.VisionRange; x < unit.VisionRange; x++)
        {
            for (int y = -unit.VisionRange; y < unit.VisionRange; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= unit.VisionRange)
                {
                    Vector2Int tile = unitPos + new Vector2Int(x, y);
                    VisibleTiles[unit.IsAlly].Add(tile);
                }
            }
        }
    }


    /// <summary>
    /// Helper method to check if a specific tile is visible to a team. Currently not used.
    /// </summary>
    /// <param name="tile">Tile to check</param>
    /// <param name="isAlly">Team to check</param>
    /// <returns>True if tile visible for that team, false if tile not visible for that team</returns>
    public bool IsTileVisible(Vector2Int tile, bool isAlly)
    {
        return VisibleTiles[isAlly].Contains(tile);
    }


    /// <summary>
    /// Get visible tiles for a team.
    /// </summary>
    /// <param name="isAlly">True to get ally visible tiles, false to get enemy visible tiles.</param>
    /// <returns>HashSet of the specific teams visible tiles.</returns>
    public HashSet<Vector2Int> GetVisibleTiles(bool isAlly)
    {
        return VisibleTiles[isAlly];
    }
}
