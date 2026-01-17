using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatResolver : MonoBehaviour
{
    public static CombatResolver Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    // Resolves combat between units on the same tile.
    // Clean this up.... horrible....
    public void ResolveCombat(List<Unit> units)
    {
        Vector2Int position = units[0].GridPos;

        Unit unitAlly = null;
        Unit unitEnemy = null;

        foreach (var unit in units)
        {
            if (unit.IsAlly)
            {
                unitAlly = unit;
            }
            else
            {
                unitEnemy = unit;
            }
        }

        // Get movement status of last tick because unmoving unit loses by default
        bool allyMoved = unitAlly.MovedThisTick;
        bool enemyMoved = unitEnemy.MovedThisTick;

        if (allyMoved && !enemyMoved)
        {
            Debug.Log($"Unit A ({unitAlly.Type}) wins against Unit B ({unitEnemy.Type})");
            DestroyUnit(unitEnemy);
        }
        else if (!allyMoved && enemyMoved)
        {
            Debug.Log($"Unit B ({unitEnemy.Type}) wins against Unit A ({unitAlly.Type})");
            DestroyUnit(unitAlly);
        }
        else if (allyMoved && enemyMoved) // If both moved, the team with control of the tile wins
        {
            List <Unit> Control = UnitsManager.Instance.CheckControlOfTile(position); // Returns 1 if ally controlled, -1 if enemy controlled, 0 if neutral
            int controlResult = CountControl(Control);

            switch (controlResult)
            {
                case > 0:
                    Debug.Log($"Unit A ({unitAlly.Type}) wins by control against Unit B ({unitEnemy.Type})");
                    DestroyUnit(unitEnemy);
                    break;
                case < 0:
                    Debug.Log($"Unit B ({unitEnemy.Type}) wins by control against Unit A ({unitAlly.Type})");
                    DestroyUnit(unitAlly);
                    break;
                case 0:
                    Debug.Log("Both units eliminated due to equal control.");
                    DestroyUnit(unitAlly);
                    DestroyUnit(unitEnemy);
                    break;
            }
        }
        else
        {
            Debug.Log("Something went wrong.");
        }
    }


    public int CountControl(List <Unit> units)
    {
        int allyControl = 0;
        int enemyControl = 0;

        foreach (var unit in units)
        {
            if (unit.IsAlly)
            {
                allyControl++;
            }
            if (!unit.IsAlly)
            {
                enemyControl++;
            }
        }

        return allyControl - enemyControl;
    }


    public void ResolveCollision(Unit unit1, Unit unit2)
    {
        if (unit1.IsAlly == unit2.IsAlly)
        {
            unit1.CancelMove();
            unit2.CancelMove();
            return;
        }

        List<Unit> pos1Control = UnitsManager.Instance.CheckControlOfTile(unit1.GridPos);
        List<Unit> pos2Control = UnitsManager.Instance.CheckControlOfTile(unit2.GridPos);

        List<Unit> totalControl = pos1Control.Union(pos2Control).ToList();

        int controlResult = CountControl(totalControl);

        Unit ally = unit1.IsAlly ? unit1 : unit2;
        Unit enemy = unit1.IsAlly ? unit2 : unit1;

        switch (controlResult)
        {
            case > 0:
                Debug.Log($"Allied units win collision between {unit1.GridPos} and {unit2.GridPos}");
                DestroyUnit(enemy);
                break;
            case < 0:
                Debug.Log($"Enemy units win collision at {unit1.GridPos} and {unit2.GridPos}");
                DestroyUnit(ally);
                break;
            case 0:
                Debug.Log("All units eliminated due to equal control in collision.");
                DestroyUnit(ally);
                DestroyUnit(enemy);
                break;
        }
    }


    public void DestroyUnit(Unit unit)
    {
        UnitsManager.Instance.UnRegisterUnit(unit);
    }
}
