using UnityEngine;


/// <summary>
/// Currently obsolete class. Used to handle unit model visual movement on board. Kept for now for possible future need.
/// </summary>
public class UnitView : MonoBehaviour
{
    private Unit _unit;

    private void Awake()
    {
        _unit = GetComponentInParent<Unit>();
    }
}
