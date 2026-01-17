using NUnit.Framework.Internal;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public int elevation;
    public Color originalColor;

    private void OnValidate()
    {
        Vector3 position = transform.position;
        gridPosition = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z)); // Calculate grid position
        elevation = Mathf.RoundToInt(position.y); // Calculate elevation

        originalColor = GetComponent<Renderer>().sharedMaterial.color; // Store the original color of tile
    }

    // Highlights a tile. Called from HighlightHover script and UnitMovementController script.
    public void Highlight()
    {
        //Debug.Log($"Tile at {gridPosition} highlighted.");
        GetComponent<Renderer>().material.color = Color.white;
    }

    // Hightlight clearing method. Called from HighlightHover script.
    public void ClearHighlight()
    {
        //Debug.Log($"Tile at {gridPosition} highlight cleared.");
        GetComponent<Renderer>().material.color = originalColor;
    }
}
