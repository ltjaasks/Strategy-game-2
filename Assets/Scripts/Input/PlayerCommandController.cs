using System;
using UnityEngine;
using UnityEngine.XR;


/// <summary>
/// Class to handle player input for selecting units, moving them, and setting attack targets.
/// </summary>
public class PlayerCommandController : MonoBehaviour
{
    [SerializeField] private PointerRaycaster pointerRaycaster;
    public static Unit SelectedUnit;
    private Tile HoverTile;
    private Vector2Int[] CurrentPath;

    /// <summary>
    /// Called once per frame. Checks player inputs and calls appropriate methods.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // Listener for T key. Used in testing to manually advance ticks.
        {
            TickManager.Instance.ManualTick();
        }
        if (Input.GetMouseButtonUp(0)) // Listener for left mouse button up. Used to select units and set move targets.
        {
            SelectUnit();
            MoveUnitTo();
        }
        if(Input.GetMouseButtonUp(1)) // Right click to set attack
        {
            AttackWithUnit();
        }

        // Highlights attack tiles of selected unit. Replaces old restoring previous highlight solution.
        if (SelectedUnit != null)
        {
            Vector2Int[] attackTiles = SelectedUnit.GetAttackTiles();
            TileHighlighter.Instance.HighlightTiles(attackTiles, Tiles.HighlightType.Attack);
        }
    }


    /// <summary>
    /// Selects the unit hovered by the pointer if it is an ally. Called when left mouse button is clicked.
    /// </summary>
    void SelectUnit()
    {
        TileHighlighter.Instance.ClearAllHighlights();
        Unit hoveredUnit = pointerRaycaster.HoveredUnit;

        if (hoveredUnit != null && hoveredUnit.IsAlly)
        {
            SelectedUnit = hoveredUnit;
            CurrentPath = null;
        }
    }


    /// <summary>
    /// Gives command to move the selected unit to the tile currently hovered by the pointer.
    /// Clears highlights and deselects unit after command is given.
    /// </summary>
    void MoveUnitTo()
    {
        if (SelectedUnit != null)
        {
            Tile hoveredTile = pointerRaycaster.HoveredTile;
            if (hoveredTile != null)
            {
                SelectedUnit.SetTargetTile(hoveredTile);
                TileHighlighter.Instance.ClearAllHighlights();
                SelectedUnit = null;
            }
        }
    }


    /// <summary>
    /// Gives command for selected unit to attack the tile currently hovered by the pointer.
    /// Clears highlights and deselects unit after command is given.
    /// </summary>
    void AttackWithUnit()
    {
        if (SelectedUnit == null)
            return;

        Tile hoveredTile = pointerRaycaster.HoveredTile;
        if (hoveredTile == null)
            return;

        SelectedUnit.SetAttackTile(hoveredTile);

        TileHighlighter.Instance.ClearAllHighlights();
        SelectedUnit = null;
    }


    void OnEnable()
    {
        pointerRaycaster.OnHoveredTileChanged += HandleHoveredTileChanged;
    }

    void OnDisable()
    {
        pointerRaycaster.OnHoveredTileChanged -= HandleHoveredTileChanged;
    }


    /// <summary>
    /// Highlights hovered tile and calls DrawPath if unit is selected.
    /// Called when pointerRaycaster detects a change in hovered tile.
    /// </summary>
    /// <param name="newTile"></param>
    private void HandleHoveredTileChanged(Tile newTile)
    {
        HoverTile?.HideHighlight();
        HoverTile = newTile;

        if (SelectedUnit != null)
        {
            DrawPath();
            return;
        }

        // Set and highlight new tile
        HoverTile?.ShowHighlight(Tiles.HighlightType.Hover);
    }


    /// <summary>
    /// Draws path the selected unit would take to move to the currently hovered tile.
    /// </summary>
    public void DrawPath()
    {
        if (SelectedUnit == null || HoverTile == null) return;
        var path = PathFinder.CalculatePath(
            SelectedUnit.GridPos,
            HoverTile.gridPosition,
            SelectedUnit
        );

        if (path == null) return;
        CurrentPath = path.ToArray();
        TileHighlighter.Instance.HighlightPath(CurrentPath);
    }
}
