using System.Collections.Generic;
using Template.Runtime;
using UnityEngine;

/// <summary>
/// A*アルゴリズム
/// </summary>
public class AStar : IAlgorithm
{
    public IReadOnlyList<Vector2Int> Search(Vector2Int start, Vector2Int goal, IReadOnlyList<string> grid)
    {
        PriorityQueue<Vector2Int, float> queue = new PriorityQueue<Vector2Int, float>();
        var closedList = new HashSet<Vector2Int>();
        var root = new List<Vector2Int>();
        int[] dx = new int[] { 0, 0, 1, -1 };
        int[] dy = new int[] { 1, -1, 0, 0 };

        closedList.Add(start);
        queue.Enqueue(start, 0);

        while (queue.Count > 0)
        {
            var openedList = new List<Vector2Int>();
            var current = queue.Dequeue();
            if (current == goal)
                break;  
            
            for (int i = 0; i < 4; i++)
            {
                var nx = dx[i] + current.x;
                var ny = dy[i] + current.y;

                if(nx < 0 || nx >= grid[0].Length || ny < 0 || ny >= grid.Count)
                    continue;

                if(closedList.Contains(new Vector2Int(nx, ny)))
                    continue;

                if(grid[ny][nx] == '#')   
                    continue;
              
                openedList.Add(new Vector2Int(nx, ny));
                closedList.Add(new Vector2Int(nx, ny));
            }

            var bestNode = current;
            var bestCost = 1000f;
            foreach (var node in openedList)
            {
                float cost = ActualCost(node, goal) + Heuristic(node, goal);
                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestNode = node;
                }
            }
            queue.Enqueue(bestNode, bestCost);
            root.Add(bestNode);
        }
        root.Reverse();
        return root;
    }
    /// <summary>
    /// 実コスト計算
    /// マンハッタン距離
    /// </summary>
    /// <param name="a"></param>
    /// <param name="goal"></param>
    /// <returns></returns>
    private int ActualCost(Vector2Int a, Vector2Int goal)
    {
        return Mathf.Abs(a.x - goal.x) + Mathf.Abs(a.y - goal.y);
    }
    /// <summary>
    /// ヒューリスティック計算
    /// ユークリッド距離
    /// </summary>
    /// <param name="a"></param>
    /// <param name="goal"></param>
    /// <returns></returns>
    private float Heuristic(Vector2Int a, Vector2Int goal)
    {
        return Mathf.Sqrt(Mathf.Pow(a.x - goal.x, 2) + Mathf.Pow(a.y - goal.y, 2));
    }
}
