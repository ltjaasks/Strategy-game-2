using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitMoves;
using Units;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitType Type;
    [SerializeField] private Vector2Int P_GridPos;
    public int Elevation;
    //public List<Vector2Int> Path = new List<Vector2Int>();
    public Tile TargetTile;
    private Vector2Int[] Path = Array.Empty<Vector2Int>();
    public int pathIndex = 0;
    public bool IsMoving = false;
    public bool IsAlly;
    public bool MovedThisTick { get; private set; }

    public int MoveCooldown { get; private set; }
    private int _cooldownRemaining = 0;

    public int Speed => UnitsData.GetSpeed(Type);

    private void Start()
    {
        SnapToGrid();
        MoveCooldown = UnitsData.GetMoveCooldown(Type);
        _cooldownRemaining = 0;

        UnitsManager.Instance.RegisterUnit(this);
    }


    public void BeginTick()
    {
        MovedThisTick = false;
    }


    public Vector2Int GridPos
    {
        get
        {
            return P_GridPos;
        }
        set
        {
            if (P_GridPos == value) return;
            var old = P_GridPos;
            P_GridPos = value;
            OnGridPosChanged?.Invoke(old, P_GridPos);
            OnGridPosChangedVisual?.Invoke(this);
        }
    }


    // Event triggered when the unit's grid position changes.
    // Parameters: old position, new position
    public event Action<Vector2Int, Vector2Int> OnGridPosChanged;
    public event Action<Unit> OnGridPosChangedVisual;


    // Aligns the unit's world position to its grid position and elevation on start.
    public void SnapToGrid()
    {
        transform.position = GridManager.Instance.GridToWorld(P_GridPos, Elevation);
    }


    public void StartMove()
    {
        //Debug.Log("Starting unit movement. Path length: " + PathToTarget.Length);
        IsMoving = true;
        pathIndex = 0; // Reset path index at the start of movement
    }


    public void CancelMove()
    {
        IsMoving = false;
        pathIndex = 0;
        Path = Array.Empty<Vector2Int>();
    }

    public void Move()
    {
        if (_cooldownRemaining > 0)
        {
            --_cooldownRemaining;
            return;
        }
        if (Path.Length == 0 || pathIndex >= Path.Length)
        {
            Path = Array.Empty<Vector2Int>();
            pathIndex = 0;
            return;
        }
        for (int i = Speed; i > 0; i--)
        {
            MoveToNext();
        }
        _cooldownRemaining = MoveCooldown;
    }


    public void MoveToNext()
    {
        if (UnitsManager.Instance.CheckTileForAllies(Path[pathIndex], this.IsAlly))
        {
            Debug.Log("Movement blocked by ally unit at " + Path[pathIndex]);
            Path = Array.Empty<Vector2Int>();
            IsMoving = false;
            pathIndex = 0;
            return;
        }

        //Debug.Log("Unit moving... " + pathIndex);
        GridPos = Path[pathIndex];
        Elevation = GridManager.Instance.GetTile(GridPos).elevation + 1;
        //transform.position = GridManager.Instance.GridToWorld(GridPos, elevation);
        pathIndex++;
        MovedThisTick = true;

        if (GridPos == Path.Last())
        {
            TargetReached();
        }
    }


    public void TargetReached()
    {
        Debug.Log("Unit reached target at " + GridPos);
        Path = Array.Empty<Vector2Int>();
        pathIndex = 0;
        IsMoving = false;
    }



    // Checks if the target position is within the allowed moves for this unit.
    Boolean IsMoveAllowed(Vector2Int targetPosition)
    {
        Vector2Int[] allowedMoves = GetAllowedMoves(P_GridPos);
        if (allowedMoves.Contains(targetPosition))
        {
            return true;
        }
        return false;
    }


    // Retrieves allowed moves based on unit type and current position from UnitsData.
    public Vector2Int[] GetAllowedMoves(Vector2Int pos)
    {
        return UnitsData.GetMoveSet(Type, pos);
    }


    // Retrieves controlled tiles based on unit type and current position from UnitsData.
    public Vector2Int[] GetControlledTiles(Vector2Int pos)
    {
        return UnitsData.GetControlSet(Type, pos);
    }


    public Vector2Int[] GetMoves()
    {
        return UnitsData.GetMoveSet(Type, GridPos);
    }


    public Vector2Int? GetNextTileInPath()
    {
        if (Path.Length == 0 || pathIndex >= Path.Length)
        {
            return null; // No movement, return current position
        }
        return Path[pathIndex];
    }


    public void SetPath(Tile tile)
    {
        Vector2Int target = tile.gridPosition;
        List<Vector2Int> pathTest = PathFinder.CalculatePath(GridPos, target, this);
        Path = pathTest.ToArray();
    }

}
