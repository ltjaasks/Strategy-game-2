using UnityEngine;

public class PointerRaycaster : MonoBehaviour
{
    public Unit HoveredUnit { get; private set; }
    private Tile _hoveredTile;


    public Tile HoveredTile
    {
        get
        {
            return _hoveredTile;
        }
        private set
        {
            if (_hoveredTile == value) return;
            _hoveredTile = value;
            OnHoveredTileChanged?.Invoke(_hoveredTile);
        }
    }

    public event System.Action<Tile> OnHoveredTileChanged;

    private Vector3 _lastMousePosition;

    // Update is called once per frame
    void Update()
    {
        /*
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Only update HoveredTile when we hit a Tile component
            if (hit.collider.TryGetComponent<Tile>(out var newTile) && newTile != null)
            {
                HoveredTile = newTile;
            }

            // Only update HoveredUnit when we hit a Unit component
            if (hit.collider.TryGetComponent<Unit>(out var newUnit) && newUnit != null)
            {
                if (HoveredUnit != newUnit)
                    HoveredUnit = newUnit;
            }

            // Note: do not set HoveredTile/HoveredUnit to null if Raycast misses.
            // This prevents issues in other systems and throws NullReferenceExceptions.
        }
        */
        if (Input.mousePosition == _lastMousePosition) return;
        _lastMousePosition = Input.mousePosition;

        Tile newTile = null;
        HoveredUnit = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            newTile = hit.collider.GetComponent<Tile>();
            HoveredUnit = hit.collider.GetComponent<Unit>();
        }

        HoveredTile = newTile;
    }
}
