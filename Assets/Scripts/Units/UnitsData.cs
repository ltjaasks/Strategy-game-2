using UnityEngine;
using Units;

namespace UnitMoves
{
    /// <summary>
    /// Data class for different information for all unit types.
    /// </summary>
    public class UnitsData
    {
        /// <summary>
        /// Get the tiles a unit can move to on the next tick.
        /// </summary>
        /// <param name="type">Unit type to check</param>
        /// <param name="fromPosition">Units current tile</param>
        /// <param name="fd">Units forward direction</param>
        /// <returns>Array of allowed moves</returns>
        public static Vector2Int[] GetMoveSet(UnitType type, Vector2Int fromPosition, int fd)
        {
            return type switch
            {
                UnitType.Infantry => new Vector2Int[]
                {
                    fromPosition + new Vector2Int(0, 1 * fd), // Front
                    fromPosition + new Vector2Int(1, 1 * fd), // Front-right diagonal
                    fromPosition + new Vector2Int(-1, 1 * fd), // Front-left diagonal
                },
                UnitType.Spearman => new Vector2Int[]
                {
                    fromPosition + new Vector2Int(0, 1 * fd), // Front
                    fromPosition + new Vector2Int(1, 1 * fd), // Front-right diagonal
                    fromPosition + new Vector2Int(-1, 1 * fd), // Front-left diagonal
                    fromPosition + new Vector2Int(1, -1 * fd), // Back-right diagonal
                    fromPosition + new Vector2Int(-1, -1 * fd), // Back-left diagonal
                },
                UnitType.Commander => new Vector2Int[]
                {
                    fromPosition + new Vector2Int(0, 1 * fd), // Front
                    fromPosition + new Vector2Int(1, 0), // Right
                    fromPosition + new Vector2Int(-1, 0), // Left
                    fromPosition + new Vector2Int(0, -1 * fd), // Back
                    fromPosition + new Vector2Int(1, 1 * fd), // Front-right diagonal
                    fromPosition + new Vector2Int(-1, 1 * fd), // Front-left diagonal
                    fromPosition + new Vector2Int(1, -1 * fd), // Back-right diagonal
                    fromPosition + new Vector2Int(-1, -1 * fd), // Back-left diagonal
                },
                UnitType.Archer => new Vector2Int[]
                {
                    fromPosition + new Vector2Int(0, 1 * fd), // Front
                    fromPosition + new Vector2Int(1, 0), // Right
                    fromPosition + new Vector2Int(-1, 0), // Left
                    fromPosition + new Vector2Int(0, -1 * fd), // Back
                },
                _ => new Vector2Int[0],
            };
        }


        /// <summary>
        /// Get the tiles a unit can attack to on the next tick.
        /// </summary>
        /// <param name="type">Unit type</param>
        /// <param name="fromPosition">Units current position</param>
        /// <param name="fd">Units forward direction</param>
        /// <returns>Array of allowed attacks</returns>
        public static Vector2Int[] GetAttackSet(UnitType type, Vector2Int fromPosition, int fd)
        {
            return type switch
            {
                UnitType.Infantry => GetMoveSet(type, fromPosition, fd),
                UnitType.Spearman => new Vector2Int[]
                {
                    fromPosition + new Vector2Int(1, 1 * fd), // Front-right diagonal
                    fromPosition + new Vector2Int(0, 1  * fd), // Front
                    fromPosition + new Vector2Int(-1, 1 * fd), // Front-left diagonal
                },
                UnitType.Commander => GetMoveSet(type, fromPosition, fd),
                UnitType.Archer => new Vector2Int[]
                {
                    fromPosition + new Vector2Int(0, 2 * fd),
                    fromPosition + new Vector2Int(1, 2 * fd),
                    fromPosition + new Vector2Int(-1, 2 * fd),
                    fromPosition + new Vector2Int(2, 2 * fd),
                    fromPosition + new Vector2Int(-2, 2 * fd),
                },
                _ => new Vector2Int[0],
            };
        }


        /// <summary>
        /// Check if unit type is ranged or not.
        /// </summary>
        /// <param name="type">Unit type</param>
        /// <returns>True if ranged, false if melee</returns>
        public static bool GetIsRanged(UnitType type)
        {
            return type switch
            {
                UnitType.Infantry => false,
                UnitType.Spearman => false,
                UnitType.Commander => false,
                UnitType.Archer => true,
                _ => false,
            };
        }


        /// <summary>
        /// Get movement cooldown for unit type.
        /// </summary>
        /// <param name="type">Unit type</param>
        /// <returns>Returns the movement cooldown after moving in ticks for a given unit type.</returns>
        public static int GetMoveCooldown(UnitType type)
        {
            return type switch
            {
                UnitType.Infantry => 10,
                UnitType.Spearman => 8,
                UnitType.Commander => 3,
                UnitType.Archer => 8,
                _ => 1,
            };
        }
    }
}
