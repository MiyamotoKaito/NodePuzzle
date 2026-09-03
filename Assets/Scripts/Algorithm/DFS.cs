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
        var visited = new bool[grid.Count, grid[0].Length];
        var root = new List<Vector2Int>();

        int[] dx = new int[] { 0, 0, 1, -1 };
        int[] dy = new int[] { 1, -1, 0, 0 };
        stack.Push(start);
        visited[start.y, start.x] = true;
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            root.Add(current);
            if (current == goal)
            {
                break;
            }
            for (int i = 0; i < 4; i++)
            {
                var next = new Vector2Int(current.x + dx[i], current.y + dy[i]);
                if (next.x < 0 || next.x >= grid[0].Length || next.y < 0 || next.y >= grid.Count || visited[next.y, next.x])
                {
                    continue;
                }
                if (grid[next.y][next.x] == '#')
                {
                    continue;
                }
                stack.Push(next);
                visited[next.y, next.x] = true;
                root.Add(next);
            }
        }
        root.Reverse();
        return root;
    }
}
