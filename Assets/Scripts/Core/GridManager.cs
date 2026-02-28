using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;


/// <summary>
/// Class to control the grid.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public Dictionary<Vector2Int, Tile> Tiles = new Dictionary<Vector2Int, Tile>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Populate the tiles dictionary with all Tile objects in the scene.
        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            Tiles[tile.gridPosition] = tile;
        }
    }


    /// <summary>
    /// Retrieves a tile at the specified grid position.
    /// </summary>
    /// <param name="position">Position to get the tile from</param>
    /// <returns>Requested tile</returns>
    public Tile GetTile(Vector2Int position)
    {
        Tiles.TryGetValue(position, out Tile tile);
        return tile;
    }

    public Dictionary<Vector2Int, Tile> GetTiles => Tiles;


    /// <summary>
    /// Converts grid position to world position, considering elevation.
    /// </summary>
    /// <param name="gridPosition">Position on grid</param>
    /// <param name="elevation">Elevation</param>
    /// <returns></returns>
    public Vector3 GridToWorld(Vector2Int gridPosition, int elevation = 0)
    {
        Tile tile = GetTile(gridPosition);
        double e = tile.elevation + 0.5;
        return new Vector3(gridPosition.x, (float)e, gridPosition.y);
    }

    /// <summary>
    /// Gets units position on the grid plus elevation
    /// </summary>
    /// <param name="pos">Units position on grid</param>
    /// <returns>Position on grid plus elevation</returns>
    public Vector3 UnitGridPosition(Vector2Int pos)
    {
        Tile tile = GetTile(pos);
        double e = tile.elevation;
        return new Vector3(pos.x, (float)e, pos.y);
    }
}
