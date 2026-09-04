using System.Collections.Generic;
using UnityEngine;

public interface IAlgorithm
{
    IReadOnlyList<Vector2Int> Search( Vector2Int start, Vector2Int goal, IReadOnlyList<string> grid);
}
