using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 深さ優先探索
/// </summary>
public class DFS : IAlgorithm
{
    public IReadOnlyList<Vector2Int> Search(Vector2Int start, Vector2Int goal, IReadOnlyList<string> grid)
    {
        var stack = new Stack<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>(); // 各セルへ「どこから来たか」

        int[] dx = new int[] { 0, 0, 1, -1 };
        int[] dy = new int[] { 1, -1, 0, 0 };

        stack.Push(start);
        visited.Add(start);

        var found = false;
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == goal)
            {
                found = true;
                break;
            }

            for (int i = 0; i < 4; i++)
            {
                var next = new Vector2Int(current.x + dx[i], current.y + dy[i]);
                if (next.x < 0 || next.x >= grid[0].Length || next.y < 0 || next.y >= grid.Count)
                {
                    continue;
                }
                if (grid[next.y][next.x] == '#')
                {
                    continue;
                }
                if (visited.Contains(next))
                {
                    continue;
                }

                visited.Add(next);
                cameFrom[next] = current; // 経路復元用に記録
                stack.Push(next);
            }
        }

        // 経路復元: goal から start へ逆に辿る
        var path = new List<Vector2Int>();
        if (!found)
        {
            return path; // 到達不能なら空
        }

        var node = goal;
        path.Add(node);
        while (node != start)
        {
            node = cameFrom[node];
            path.Add(node);
        }
        path.Reverse(); // start .. goal の順（各要素が隣接）
        return path;
    }
}