using Actions;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitMoves;
using Units;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Represents a unit on the game board. Contains units type, position, path, team, movement cooldown, vision
/// and other relevant information. Contains logic for unit movement, attack validation and events
/// triggered on movement and attack. Registered in UnitsManager on creation and unregistered on destruction.
/// </summary>
public class Unit : MonoBehaviour
{
    // Units type e.g. Infantry, Spearman etc.
    // Movement/attack patterns and other information are based on unit type and retrieved from UnitsData.
    public UnitType Type;

    // Unit position
    [SerializeField] private Vector2Int _gridPos;
    public int Elevation; // Elevation not relevant in current version

    // Pathfinding and movement attributes
    public Vector2Int TargetTile;
    public Vector2Int CurrentTile;
    public Vector2Int? AttackTarget;
    Queue<Vector2Int> Path = new Queue<Vector2Int>();

    // Forward direction determines the direction of unit movement and attack patterns. 1 for allies, -1 for enemies.
    public int ForwardDirection { get; private set; }

    // Current intended action of the unit. Used for determining movement and attack behavior in TickResolver.
    public ActionType MoveType { get; set; } = ActionType.Wait;

    // Unit team true for allies, false for enemies
    public bool IsAlly;

    // Move cooldown determines how many ticks unit has to wait after performing
    // a move or attack before it can move or attack again.
    public int MoveCooldown { get; private set; }
    private int _moveCooldown = 0;

    // Ranged/Melee determines, if moves on attack or not
    private bool _isRanged;

    // Range of vision. Vision clears fog of war and allows player to see tile and units on it.
    public int VisionRange = 3;

    // Indicates if the unit is currently moving. Used for movement animations.
    public bool IsMoving { get; private set; }

    // Parent object for unit models. Used to enable/disable visibility of the unit.
    [SerializeField] private GameObject _modelRoot;


    // Initialization of the unit. Registers unit in UnitsManager,
    // sets move cooldown and forward direction based on team.
    private void Start()
    {
        SnapToGrid();
        MoveCooldown = UnitsData.GetMoveCooldown(Type);
        _moveCooldown = 0;
        _isRanged = UnitsData.GetIsRanged(Type);

        UnitsManager.Instance.RegisterUnit(this);

        ForwardDirection = IsAlly ? 1 : -1;

        if (_modelRoot == null)
            _modelRoot = transform.Find("ModelRoot").gameObject;
    }


    /// <summary>
    /// Sets the visibility of the model root object.
    /// </summary>
    /// <param name="visible"><see langword="true"/> to make the model root visible; otherwise, <see langword="false"/> to hide it.</param>
    public void SetVisible(bool visible)
    {
        _modelRoot.SetActive(visible);
    }


    /// <summary>
    /// Sets the target tile for the entity and calculates a movement path to that tile.
    /// </summary>
    /// <remarks>This method updates the entity's movement path and sets its action type to move. Any
    /// previously set path will be replaced.</remarks>
    /// <param name="tile">The destination <see cref="Tile"/> to move toward. Cannot be <c>null</c>.</param>
    public void SetTargetTile(Tile tile)
    {
        TargetTile = tile.gridPosition;

        Vector2Int[] tempPath = PathFinder.CalculatePath(GridPos, TargetTile, this).ToArray();
        Path = new Queue<Vector2Int>(tempPath);

        MoveType = ActionType.Move;
    }


    /// <summary>
    /// Attempts to set the specified tile as the target for an attack action.
    /// </summary>
    /// <param name="tile">Tile targeted by the attack</param>
    /// <returns>True if attack is successful, false if attck is unsuccessful</returns>
    public bool SetAttackTile(Tile tile)
    {
        bool canAttack = ValidateAttack(tile.gridPosition);
        if (!canAttack)
        {
            return false;
        }

        TargetTile = tile.gridPosition;
        AttackTarget = tile.gridPosition;

        // Ranged units do not move to attack, so path is set to current position.
        // Melee units move to target tile to attack, so path is set to target tile.
        if (_isRanged)
        {
            Path = new Queue<Vector2Int>(new Vector2Int[] { GridPos });
        }
        else
            Path = new Queue<Vector2Int>(new Vector2Int[] { TargetTile });

        MoveType = ActionType.Attack;
        return true;
    }


    /// <summary>
    /// Get units intended position on next tick
    /// </summary>
    /// <returns>Vector2Int of intended next tile</returns>
    public Vector2Int GetNextTile()
    {
        if (Path.Count == 0) return GridPos;
        if (MoveType == ActionType.Wait || _moveCooldown > 0) return GridPos;
        return Path.Peek();
    }


    /// <summary>
    /// Get units current position on the grid
    /// </summary>
    public Vector2Int GridPos
    {
        get
        {
            return _gridPos;
        }
        set
        {
            if (_gridPos == value) return;
            var old = _gridPos;
            _gridPos = value;
            OnGridPosChanged?.Invoke(old, _gridPos);
            OnGridPosChangedVisual?.Invoke(this);
        }
    }


    // Event triggered when the unit's grid position changes.
    // Parameters: old position, new position
    public event Action<Vector2Int, Vector2Int> OnGridPosChanged;
    public event Action<Unit> OnGridPosChangedVisual;


    /// <summary>
    /// Aligns the unit's world position with its grid position.
    /// </summary>
    public void SnapToGrid()
    {
        transform.position = GridManager.Instance.UnitGridPosition(GridPos);
        //transform.position = GridManager.Instance.GridToWorld(_gridPos, Elevation);
    }


    /// <summary>
    /// Retrieves the set of tiles the unit can currently attack to based on it's type from UnitsData
    /// </summary>
    /// <returns>Vector2Int[] of allowed attack tiles</returns>
    public Vector2Int[] GetAttackTiles()
    {
        return UnitsData.GetAttackSet(Type, GridPos, ForwardDirection);
    }


    /// <summary>
    /// Retrieves the set of tiles the unit can currently move to on the next tick based on it's type from UnitsData
    /// </summary>
    /// <returns>Vector2Int[] of allowed move tiles</returns>
    public Vector2Int[] GetMoveTiles()
    {
        return UnitsData.GetMoveSet(Type, GridPos, ForwardDirection);
    }


    /// <summary>
    /// Checks if the target tile is in units current attack tiles and if the tile is occupied by enemy
    /// </summary>
    /// <param name="target">Tile targeted by attack</param>
    /// <returns>Returns true if the attack is valid, false otherwise.</returns>
    public bool ValidateAttack(Vector2Int target)
    {
        if (!GetAttackTiles().Contains(target)) // Check if target is in attack tiles
        {
            Debug.Log($"Target {target} is not in attack tiles.");
            return false;
        }

        if (!UnitsManager.Instance.CheckTileForEnemy(target, IsAlly)) // Check if target tile has enemy
        {
            Debug.Log($"Target {target} does not contain enemy.");
            return false;
        }

        if (_moveCooldown > 0) // Check if unit is on move cooldown
        {
            Debug.Log($"Unit is on move cooldown for {_moveCooldown} more ticks.");
            return false;
        }

        return true;
    }


    /// <summary>
    /// Advances the unit along its current path, performing movement or attack actions as appropriate.
    /// </summary>
    /// <remarks>This method processes the unit's movement turn by moving it to the next position in its path,
    /// triggering movement or attack events as needed. If a movement cooldown is active, the cooldown is decremented
    /// and no movement occurs until it reaches zero. When the unit moves, relevant events are raised to notify
    /// listeners of movement or attack actions. Called after TickResolver has resolved next ticks positions</remarks>
    public void ApplyMove()
    {
        // Check units move cooldown
        if (_moveCooldown > 0)
        {
            _moveCooldown--;
            if (_moveCooldown == 0)
                MoveType = ActionType.Move;
            return;
        }

        // If unit has no path, do nothing
        if (Path.Count == 0) return;

        // Move unit to next position in path and advance path
        GridPos = Path.Dequeue();

        // Trigger movement event with new world position
        Vector3 worldPos = GridManager.Instance.UnitGridPosition(GridPos);

        OnMoveRequested?.Invoke(worldPos);

        if (MoveType == ActionType.Attack)
        {
            PerformAttack();
        }
        if (MoveType == ActionType.Move)
        {
            IsMoving = true;
            OnMovingStateChanged?.Invoke(true);
        }

        MoveType = ActionType.Wait;
        _moveCooldown = MoveCooldown;
    }


    // Events related to unit movement. Trigger animations and moving of models on the board.
    public event Action<Vector3> OnMoveRequested;
    public event Action<bool> OnMovingStateChanged;

    public event Action OnAttackRequested;

    public void PerformAttack()
    {
        OnAttackRequested?.Invoke();
    }

    /// <summary>
    /// Returns IsMoving to false to return model to idle animation
    /// </summary>
    public void NotifyMovementFinished()
    {
        IsMoving = false;
        OnMovingStateChanged?.Invoke(false);
    }


    /// <summary>
    /// Cancels unit's current action. Called after TickResolver determines action impossible
    /// e.g. due to same team units colliding. Clears unit's path and sets action to wait.
    /// </summary>
    public void ApplyCancel()
    {
        Path.Clear();
        MoveType = ActionType.Wait;
    }


    /// <summary>
    /// Unimplemented mechanic. In the future handles units attacking each other at the same time.
    /// </summary>
    public void ApplyEngage()
    {
        // Placeholder
        return;
    }


    /// <summary>
    /// Handles the death of the unit, performing any necessary cleanup and triggering end-of-game logic if applicable.
    /// </summary>
    /// <remarks>If the unit is a commander, this method ends the game and determines the winner based on the
    /// unit's allegiance. In all cases, the unit is unregistered from the unit manager.</remarks>
    public void ApplyDeath()
    {
        if (Type == UnitType.Commander)
        {
            bool playerWon = !IsAlly;

            GameManager.Instance.EndGame(playerWon);
        }
        UnitsManager.Instance.UnRegisterUnit(this);
    }
}
