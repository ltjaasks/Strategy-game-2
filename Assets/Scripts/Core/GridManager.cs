using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();

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
            tiles[tile.gridPosition] = tile;
        }

        Debug.Log("Count of tiles: " + GridManager.Instance.tiles.Count);
    }


    // Retrieves a tile at the specified grid position.
    public Tile GetTile(Vector2Int position)
    {
        tiles.TryGetValue(position, out Tile tile);
        return tile;
    }


    // Converts grid position to world position, considering elevation.
    public Vector3 GridToWorld(Vector2Int gridPosition, int elevation = 0)
    {
        return new Vector3(gridPosition.x, elevation, gridPosition.y);
    }


    public void ClearHighlights()
    {
        foreach (var tile in tiles.Values)
        {
            tile.ClearHighlight();
        }
    }
}
