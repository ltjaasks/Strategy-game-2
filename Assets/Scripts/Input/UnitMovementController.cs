using System;
using UnityEngine;
using UnityEngine.XR;

public class UnitMovementController : MonoBehaviour
{
    [SerializeField] private PointerRaycaster pointerRaycaster;
    public static Unit SelectedUnit;

    // TODO: clean up
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            SelectUnit();
            MoveUnitTo();
        }
    }

    // Selects the unit currently hovered by the pointer.
    void SelectUnit()
    {
        Unit hoveredUnit = pointerRaycaster.HoveredUnit;
        if (hoveredUnit != null && !hoveredUnit.IsMoving && hoveredUnit.IsAlly)
        {
            GridManager.Instance.ClearHighlights();
            SelectedUnit = hoveredUnit;
            //HighlightAllowedMoves(SelectedUnit.GetAllowedMoves());
        }
    }


    // Moves the selected unit to the tile hovered by the pointer. Called when left mouse button is clicked.
    // Calls the MoveTo method of the Unit class.
    void MoveUnitTo()
    {
        if (SelectedUnit != null)
        {
            Tile hoveredTile = pointerRaycaster.HoveredTile;
            if (hoveredTile != null)
            {
                SelectedUnit.SetPath(hoveredTile);
                GridManager.Instance.ClearHighlights();
                SelectedUnit = null;
            }
        }
    }
}
