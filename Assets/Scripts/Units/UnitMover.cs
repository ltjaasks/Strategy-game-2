using UnityEngine;


/// <summary>
/// Moves unit models in game space when they move logically on the grid.
/// Subscribes to unit's move requests and moves the unit towards the requested position.
/// </summary>
public class UnitMover : MonoBehaviour
{
    // Movespeed of model
    [SerializeField] private float _moveSpeed = 1f;

    private Vector3 _targetPosition;
    private bool _isMoving;

    private Unit _unit;


    private void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    /// <summary>
    /// Subscribes to the unit's move requests when enabled, and unsubscribes when disabled.
    /// </summary>
    private void OnEnable()
    {
        _unit.OnMoveRequested += MoveTo;
    }

    private void OnDisable()
    {
        _unit.OnMoveRequested -= MoveTo;
    }


    /// <summary>
    /// Moves unit model in game space towards the target position when _isMoving is true.
    /// When the unit reaches the target position, it sets _isMoving to false and notifies
    /// the unit that movement is finished.
    /// </summary>
    private void Update()
    {
        if (!_isMoving)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetPosition,
            _moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
        {
            transform.position = _targetPosition;
            _isMoving = false;

            _unit.NotifyMovementFinished();
        }
    }


    /// <summary>
    /// Sets the target position for the unit model to move towards and sets _isMoving to true.
    /// </summary>
    /// <param name="worldPosition">Target of movement</param>
    private void MoveTo(Vector3 worldPosition)
    {
        _targetPosition = worldPosition;
        _isMoving = true;
    }
}
