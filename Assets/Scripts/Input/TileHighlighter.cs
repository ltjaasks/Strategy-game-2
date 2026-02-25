using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Tiles;
using UnityEngine;
using UnityEngine.TerrainUtils;


/// <summary>
/// Class to handle highlighting multiple tiles collectively.
/// </summary>
public class TileHighlighter : MonoBehaviour
{
    public static TileHighlighter Instance;
    private Vector2Int[] CurrentPath;
    private Vector2Int AttackTiles;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }


    /// <summary>
    /// Highlights the given tiles with the specified highlight type.
    /// </summary>
    /// <param name="tiles">Tiles to highlight</param>
    /// <param name="type">Highlight type to use</param>
    public void HighlightTiles(Vector2Int[] tiles, HighlightType type)
    {
        foreach (Vector2Int tile in tiles)
        {
            Tile t = GridManager.Instance.GetTile(tile);
            if (t != null)
            {
                //t.Highlight(type);
                t.ShowHighlight(type);
            }
        }
    }


    /// <summary>
    /// Clears all highlights on the board.
    /// </summary>
    public void ClearAllHighlights()
    {
        foreach (var kvp in GridManager.Instance.Tiles)
        {
            Tile tile = kvp.Value;
            //tile.ClearHighlight();
            tile.HideHighlight();
        }
    }


    /// <summary>
    /// Highlights the given path and stores it as current path.
    /// </summary>
    /// <param name="path">Path to highlight, Vector2Int[]</param>
    public void HighlightPath(Vector2Int[] path)
    {
        if (CurrentPath != null)
        {
            ClearPath(CurrentPath);
        }
        CurrentPath = path;
        HighlightTiles(path, HighlightType.Path);
    }


    /// <summary>
    /// Clears the highlights on the given path and resets current path.
    /// </summary>
    /// <param name="tiles">Tiles to clear</param>
    public void ClearPath(Vector2Int[] tiles)
    {
        foreach (Vector2Int tile in tiles)
        {
            Tile t = GridManager.Instance.GetTile(tile);
            if (t != null)
            {
                t.HideHighlight();
            }
        }
        CurrentPath = null;
    }

}
