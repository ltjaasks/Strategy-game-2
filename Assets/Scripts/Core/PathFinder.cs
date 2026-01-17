using System.Collections.Generic;
using UnitMoves;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    /*
    public Vector2Int[] GetPath(Unit unit, Vector2Int target)
    {
        return CalculatePath(unit.GridPos, target, unit).ToArray();
    }
    */

    public static List<Vector2Int> CalculatePath(Vector2Int start, Vector2Int target, Unit unit)
    {
        int safetyCounter = 0;

        Queue<Vector2Int> frontier = new Queue<Vector2Int>(); // frontier is the queue of positions to explore

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>(); // for reconstructing the path

        frontier.Enqueue(start); // Queue the starting position
        visited.Add(start); // Mark the starting position as visited

        while (frontier.Count > 0) // While there are positions to explore
        {
            safetyCounter++;
            if (safetyCounter > 10_000)
            {
                Debug.LogError("BFS aborted: too many iterations");
                return null;
            }

            Vector2Int current = frontier.Dequeue();
            if (current == target)
            {
                break;
            }

            foreach (Vector2Int move in UnitsData.GetMoveSet(unit.Type, current))
            {
                Vector2Int next = move;

                if (visited.Contains(next)) continue;

                frontier.Enqueue(next);
                visited.Add(next);
                cameFrom[next] = current;
            }
        }

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int step = target;

        if (!cameFrom.ContainsKey(step))
        {
            return null; // No path found
        }

        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }

        path.Reverse();
        return path;
    }
}
