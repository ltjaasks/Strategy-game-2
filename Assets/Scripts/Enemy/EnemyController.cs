using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class to control enemy units' behavior.
/// </summary>
public class EnemyController : MonoBehaviour
{
    private Unit[] EnemyUnits;
    public static EnemyController Instance;

    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    /// <summary>
    /// Moves enemy units. Currently enemies attack if possible and move randomly otherwise.
    /// TODO: Smart AI
    /// </summary>
    public void MoveEnemies()
    {
        EnemyUnits = UnitsManager.Instance.GetUnitsByFaction(false);


        foreach (Unit unit in EnemyUnits)
        {
            Vector2Int[] moveTiles = unit.GetMoveTiles();
            Vector2Int[] attackTiles = unit.GetAttackTiles();

            Vector2Int? attackTarget = CanAttack(attackTiles);

            // If there is a valid attack target, set the attack tile and skip to the next unit
            if (attackTarget != null)
            {
                unit.SetAttackTile(GridManager.Instance.GetTile(attackTarget.Value));
                continue;
            }

            // Get a random tile from the moveTiles array
            Tile targetTile = GridManager.Instance.GetTile(moveTiles[Random.Range(0, moveTiles.Length)]);

            if (targetTile == null)
                continue;

            unit.SetTargetTile(targetTile);
        }
    }


    /// <summary>
    /// Checks if any of the given attack tiles contain an enemy unit.
    /// </summary>
    /// <param name="attackTiles">Units attackTiles</param>
    /// <returns>Tile the unit can attack to or null if no such tile</returns>
    public Vector2Int? CanAttack(Vector2Int[] attackTiles)
    {
        foreach (Vector2Int tile in attackTiles)
        {
            if (UnitsManager.Instance.CheckTileForEnemy(tile, false))
            {
                return tile;
            }
        }
        return null;
    }
}
