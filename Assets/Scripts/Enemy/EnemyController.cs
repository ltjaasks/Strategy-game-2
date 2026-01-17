using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private int _patrolX = 5;

    private Unit[] EnemyUnits;
    void Start()
    {
        StartCoroutine(EnemiesStartDelayed());
    }

    private IEnumerator EnemiesStartDelayed()
    {
        // Wait one frame to ensure all units are registered
        yield return new WaitForSeconds(1f);
        EnemiesStart();
    }

    // Do event based solution later. For now something simpler.
    /*
    private void OnEnable()
    {
        UnitsManager.Instance.OnUnitRegistered += OnUnitRegistered;
    }

    private void OnDisable()
    {
        UnitsManager.Instance.OnUnitRegistered -= OnUnitRegistered;
    }

    private void OnUnitRegistered(Unit unit)
    {
        if (!unit.IsAlly)
        {
            Debug.Log("Enemy added to e: " + unit.Type);
            EnemyUnits.Add(unit);
            Patrol(unit);
        }
    }
    */

    private void EnemiesStart()
    {
        Debug.Log("Enemies start runs now.");

        EnemyUnits = UnitsManager.Instance.GetUnitsByFaction(false);

        Debug.Log("Enemy units count: " + EnemyUnits.Length);

        if (!(EnemyUnits.Length > 0))
        {
            return;
        }

        InvokeRepeating(nameof(PatrolEnemy1), 1f, 5f);
    }

    private void PatrolEnemy1()
    {
        Unit enemy1 = EnemyUnits[0];
        Patrol(enemy1);
    }

    private void Patrol(Unit unit)
    {
        _patrolX = _patrolX == 0 ? 5 : 0;

        unit.SetPath(GridManager.Instance.GetTile(new Vector2Int(_patrolX, unit.GridPos.y)));

    }
}
