using NUnit.Framework.Internal;
using UnityEngine;
using Tiles;


/// <summary>
/// Class represents a single tile on the grid.
/// </summary>
public class Tile : MonoBehaviour
{
    // Tile position properties
    public Vector2Int gridPosition;
    public int elevation;

    // Highlighting properties
    private Color _previousColor;
    private Color _invisible; // Base color
    public HighlightType CurrentHighlight;


    [SerializeField] private GameObject HighlightOverlay;
    [SerializeField] private GameObject FogOverlay;

    // Highlight colors
    [SerializeField] private Color _hoverColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color _attackColor = new Color(1f, 0f, 0f, 0.4f);
    [SerializeField] private Color _pathColor = new Color(0f, 0.7f, 1f, 0.3f);


    /// <summary>
    /// Initializes the tile's grid position and elevation based on its world position,
    /// and sets the highlight overlay to inactive.
    /// </summary>
    private void OnValidate()
    {
        Vector3 position = transform.position;
        gridPosition = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z)); // Calculate grid position
        elevation = Mathf.RoundToInt(position.y); // Calculate elevation

        HighlightOverlay.SetActive(false);
    }


    /// <summary>
    /// Activates the highlight overlay and sets its color based on the specified highlight type.
    /// </summary>
    /// <param name="type">HighlightType e.g. hover</param>
    public void ShowHighlight(HighlightType type)
    {
        var renderer = HighlightOverlay.GetComponent<MeshRenderer>();
        _previousColor = renderer.material.color; // Store the current color before changing it
        CurrentHighlight = type;
        switch (type)
        {
            case HighlightType.Hover:
                renderer.material.color = _hoverColor;
                break;
            case HighlightType.Attack:
                renderer.material.color = _attackColor;
                break;
            case HighlightType.Path:
                renderer.material.color = _pathColor;
                break;
            default:
                HideHighlight();
                break;
        }

        HighlightOverlay.SetActive(true);
    }


    /// <summary>
    /// Hides highlight overlay.
    /// </summary>
    public void HideHighlight()
    {
        HighlightOverlay.SetActive(false);
    }


    /// <summary>
    /// Changes the tiles highlight color back to the previous color before the last highlight was applied.
    /// Currently not used. It was used to solve a bug where hovering for unit path wiped out the attack highlight.
    /// Kept for possible future need.
    /// </summary>
    public void RevertToPreviousColor()
    {
        var renderer = HighlightOverlay.GetComponent<MeshRenderer>();
        renderer.material.color = _previousColor; // Revert to the previous color
    }


    /// <summary>
    /// Activates/inactivates the fog overlay based on whether the tile is visible to allies or not.
    /// </summary>
    /// <param name="active"></param>
    public void SetFog(bool active)
    {
        if (active)
        {
            FogOverlay.SetActive(!active);
        }
        else
        {
            FogOverlay.SetActive(!active);
        }
    }
}
