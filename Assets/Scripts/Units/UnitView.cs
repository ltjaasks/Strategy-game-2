using UnityEngine;

public class UnitView : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;

    private Unit unit;
    private Vector3 targetWorldPos;

    void Awake()
    {
        unit = GetComponent<Unit>();
        targetWorldPos = transform.position;
    }

    void OnEnable()
    {
        unit.OnGridPosChangedVisual += HandleGridPosChanged;
    }

    void OnDisable()
    {
        unit.OnGridPosChangedVisual -= HandleGridPosChanged;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWorldPos,
            moveSpeed * Time.deltaTime
        );
    }

    void HandleGridPosChanged(Unit unit)
    {
        Vector2Int? temp = unit.GetNextTileInPath();

        if (temp == null)
        {
            return; // Skip units with no next tile
        }

        Vector2Int next = temp.Value;

        targetWorldPos = GridManager.Instance.GridToWorld(
            unit.GridPos,
            GridManager.Instance.GetTile(next).elevation + 1
        );
    }
}