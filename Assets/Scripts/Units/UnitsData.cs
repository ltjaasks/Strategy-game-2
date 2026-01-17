using UnityEngine;
using Units;

namespace UnitMoves
{
    public class UnitsData
    {

        public static Vector2Int[] GetMoveSet(UnitType type, Vector2Int fromPosition)
        {
            return type switch
            {
                UnitType.Infantry => new Vector2Int[]
                {
                    fromPosition + new Vector2Int(1, 0),
                    fromPosition + new Vector2Int(-1, 0),
                    fromPosition + new Vector2Int(0, 1),
                    fromPosition + new Vector2Int(0, -1),
                    fromPosition + new Vector2Int(1, 1),
                    fromPosition + new Vector2Int(1, -1),
                    fromPosition + new Vector2Int(-1, 1),
                    fromPosition + new Vector2Int(-1, -1),
                },
                UnitType.Cavalry => new Vector2Int[]
                {
                    fromPosition + new Vector2Int(2, 0),
                    fromPosition + new Vector2Int(-2, 0),
                    fromPosition + new Vector2Int(0, 2),
                    fromPosition + new Vector2Int(0, -2),
                    fromPosition + new Vector2Int(1, 1),
                    fromPosition + new Vector2Int(-1, -1),
                    fromPosition + new Vector2Int(1, -1),
                    fromPosition + new Vector2Int(-1, 1)
                },
                UnitType.Charger => new Vector2Int[]
                {
                    fromPosition + new Vector2Int(1, 1),
                    fromPosition + new Vector2Int(-1, 1),
                    fromPosition + new Vector2Int(1, -1),
                    fromPosition + new Vector2Int(-1, -1)
                },
                _ => new Vector2Int[0],
            };
        }

        public static Vector2Int[] GetControlSet(UnitType type, Vector2Int fromPosition)
        {
            return type switch
            {
                UnitType.Infantry => GetMoveSet(type, fromPosition),
                UnitType.Cavalry => new Vector2Int[]
                {
                    fromPosition + new Vector2Int(1, 0),
                    fromPosition + new Vector2Int(-1, 0),
                    fromPosition + new Vector2Int(0, 1),
                    fromPosition + new Vector2Int(0, -1),
                },
                _ => new Vector2Int[0],
            };
        }


        // Returns the movement range per game tick for a given unit type.
        public static int GetMoveRange(UnitType type)
        {
            return type switch
            {
                UnitType.Infantry => 1,
                UnitType.Cavalry => 2,
                _ => 0,
            };
        }


        // Returns the movement cooldown after moving in ticks for a given unit type.
        public static int GetMoveCooldown(UnitType type)
        {
            return type switch
            {
                UnitType.Infantry => 2,
                UnitType.Cavalry => 1,
                UnitType.Charger => 2,
                _ => 1,
            };
        }


        // Returns the movement speed for a given unit type. Speed is moved tiles per game tick.
        public static int GetSpeed(UnitType type)
        {
            return type switch
            {
                UnitType.Infantry => 1,
                UnitType.Cavalry => 5,
                UnitType.Charger => 3,
                _ => 1,
            };
        }
    }
}
