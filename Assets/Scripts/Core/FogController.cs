using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls rendering of fog of war based on ally units' vision.
/// </summary>
public class FogController : MonoBehaviour
{
    public static FogController Instance;

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
    /// Updates the fog of war based on ally units' vision
    /// </summary>
    public void UpdateFog()
    {
        Dictionary<Vector2Int, Tile> allTiles = GridManager.Instance.GetTiles;
        // Tiles visible to allies
        HashSet<Vector2Int> visibleTiles = VisionManager.Instance.GetVisibleTiles(true);

        foreach (var tile in allTiles.Values)
        {
            if (visibleTiles.Contains(tile.gridPosition))
            {
                tile.SetFog(true); // Visible
            }
            else
            {
                tile.SetFog(false); // Fogged
            }
        }

        UpdateUnitVisibility(visibleTiles);
    }


    /// <summary>
    /// Sets enemy units visible if the are on tiles visible to allies.
    /// </summary>
    /// <param name="visibleTiles">Tiles visible to allies.</param>
    public void UpdateUnitVisibility(HashSet<Vector2Int> visibleTiles)
    {
        foreach (Unit unit in UnitsManager.Instance.GetUnits)
        {
            if (!unit.IsAlly)
            {
                bool visible = visibleTiles.Contains(unit.GridPos);
                unit.SetVisible(visible);
            }
        }
    }
}
