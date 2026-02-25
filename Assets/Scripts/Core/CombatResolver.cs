using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Currently obsolete class.
/// </summary>
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


    public void DestroyUnit(Unit unit)
    {
        UnitsManager.Instance.UnRegisterUnit(unit);
    }
}
