using System.Collections.Generic;
using Template.Runtime;
using UnityEngine;

/// <summary>
/// A*アルゴリズム。start から goal までの最短経路を返す。
/// 返す経路は先頭が start、末尾が goal で、各要素は隣接している。
/// </summary>
public class AStar : IAlgorithm
{
    public IReadOnlyList<Vector2Int> Search(Vector2Int start, Vector2Int goal, IReadOnlyList<string> grid)
    {
        var openSet = new PriorityQueue<Vector2Int, float>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>(); // 各セルへ「どこから来たか」
        var gScore = new Dictionary<Vector2Int, int>();          // start からの実コスト(歩数)

        int[] dx = new int[] { 0, 0, 1, -1 };
        int[] dy = new int[] { 1, -1, 0, 0 };

        gScore[start] = 0;
        openSet.Enqueue(start, Heuristic(start, goal));

        var found = false;
        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();
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

                // 現在地経由でのコスト。既知の経路より短いときだけ更新する
                var tentativeG = gScore[current] + 1;
                if (gScore.TryGetValue(next, out var knownG) && tentativeG >= knownG)
                {
                    continue;
                }

                cameFrom[next] = current;
                gScore[next] = tentativeG;
                openSet.Enqueue(next, tentativeG + Heuristic(next, goal));
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
        path.Reverse();
        return path;
    }

    /// <summary>
    /// ヒューリスティック(マンハッタン距離)。
    /// </summary>
    private int Heuristic(Vector2Int a, Vector2Int goal)
    {
        return Mathf.Abs(a.x - goal.x) + Mathf.Abs(a.y - goal.y);
    }
}
