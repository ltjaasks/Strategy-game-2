using System;
using UnityEngine;
using UnityEngine.TerrainUtils;

public class HighlightHover : MonoBehaviour
{
    private Tile hoverTile;
    [SerializeField] private PointerRaycaster pointerRaycaster;
    

    void OnEnable()
    {
        pointerRaycaster.OnHoveredTileChanged += HandleHoveredTileChanged;
    }

    void OnDisable()
    {
        pointerRaycaster.OnHoveredTileChanged -= HandleHoveredTileChanged;
    }

    private void HandleHoveredTileChanged(Tile newTile)
    {
        hoverTile?.ClearHighlight();

        // Set and highlight new tile
        hoverTile = newTile;
        hoverTile?.Highlight();

        Unit selected = UnitMovementController.SelectedUnit;

        if (selected == null || hoverTile == null) return;

        GridManager.Instance.ClearHighlights();

        var path = PathFinder.CalculatePath(
            selected.GridPos,
            hoverTile.gridPosition,
            selected
        );

        if (path == null) return;

        DrawPath(path.ToArray());
    }


    public void DrawPath(Vector2Int[] path)
    {
        //Debug.Log("Drawing path...");
        foreach (Vector2Int step in path)
        {
            Tile tile = GridManager.Instance.GetTile(step);
            if (tile != null)
            {
                tile.Highlight();
            }
        }
    }
}
