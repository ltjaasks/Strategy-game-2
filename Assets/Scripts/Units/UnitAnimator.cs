using UnityEngine;


/// <summary>
/// Plays animations based on unit state.
/// </summary>
public class UnitAnimator : MonoBehaviour
{
    private Animator _animator;
    private Unit _unit;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _unit = GetComponentInParent<Unit>();
    }


    private void OnEnable()
    {
        _unit.OnMovingStateChanged += HandleMovingStateChanged;
        _unit.OnAttackRequested += HandleAttackRequested;

        // Initial state
        _animator.SetBool("IsMoving", _unit.IsMoving);
    }


    private void OnDisable()
    {
        _unit.OnMovingStateChanged -= HandleMovingStateChanged;
        _unit.OnAttackRequested -= HandleAttackRequested;
    }


    /// <summary>
    /// Sets the "IsMoving" parameter of the animator based on the unit's moving state.
    /// </summary>
    /// <param name="moving"></param>
    private void HandleMovingStateChanged(bool moving)
    {
        _animator.SetBool("IsMoving", moving);
    }

    /// <summary>
    /// Sets the "Attack" trigger of the animator when unit is attacking.
    /// </summary>
    private void HandleAttackRequested()
    {
        _animator.SetTrigger("Attack");
    }
}
