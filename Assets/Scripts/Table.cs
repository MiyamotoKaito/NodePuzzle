using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(GridLayoutGroup))]
public class Table : MonoBehaviour
{
    private const string MAPPATH = "Maps";
    private TextAsset[] _maps;
    [SerializeField] private Dictionary<CellType, Sprite> _cellSprites;
    private Dictionary<int, List<string>> _gridDic;

    private GridLayoutGroup _gridLayoutGroup;
    private List<string> _currentMap;
    private int _currentMapIndex = -1;
    private Vector2Int _start;
    private Vector2Int _goal;
    private Vector2Int _selected;
    private Cell[,] _cells;
    private readonly List<Cell> _cellPool = new List<Cell>();

    /// <summary>現在のマップ(壁/床の文字グリッド)。</summary>
    public IReadOnlyList<string> CurrentMap => _currentMap;
    /// <summary>現在選択中のノード座標。</summary>
    public Vector2Int SelectedPosition => _selected;
    /// <summary>ゴール座標。</summary>
    public Vector2Int GoalPosition => _goal;
    /// <summary>マップをクリア(ゴール到達)した瞬間に発火する。</summary>
    public event Action OnMapCleared;

    public void LoadMap()
    {
        _currentMap = GetMap();
        _gridLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
        _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayoutGroup.constraintCount = _currentMap[0].Length;
    }
    private List<string> GetMap()
    {
        var randomIndex = UnityEngine.Random.Range(0, _gridDic.Count);

        if (randomIndex == _currentMapIndex)
        {
            return GetMap();
        }

        var maps = _gridDic[randomIndex];
        _currentMapIndex = randomIndex;
        return maps;
    }
    private bool TrySetMaps()
    {
        try
        {
            // Resources/Maps 配下の全TextAssetを読み込む
            _maps = Resources.LoadAll<TextAsset>(MAPPATH);
            if (_maps == null || _maps.Length == 0)
            {
                Debug.LogError($"'Resources/{MAPPATH}' にマップが見つかりません。");
                return false;
            }
            for (int i = 0; i < _maps.Length; i++)
            {
                var map = _maps[i];
                var mapLines = new List<string>(map.text.Replace("\r", "").Split('\n').Where(line => !string.IsNullOrEmpty(line)));
                _gridDic[i] = mapLines;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"テキスト読み込み失敗: {e.Message}");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 開始地点とゴール地点を決定する。
    /// ランダムなEmptyセルを開始地点とし、そこからBFSで最も遠いEmptyセル群の中から
    /// ランダムに1つ選んでゴールとする。
    /// </summary>
    private void DecideStartAndGoal()
    {
        // Emptyセルを収集
        var emptyCells = new List<Vector2Int>();
        for (int y = 0; y < _currentMap.Count; y++)
        {
            for (int x = 0; x < _currentMap[y].Length; x++)
            {
                if (_currentMap[y][x] == '.')
                {
                    emptyCells.Add(new Vector2Int(x, y));
                }
            }
        }

        // 開始地点をランダムに決定
        _start = emptyCells[UnityEngine.Random.Range(0, emptyCells.Count)];

        // 開始地点からの距離(歩数)をBFSで算出
        var distance = BuildDistanceMap(_start);

        // 到達可能なセルの中で最大距離(最長半径)を求める
        var maxDistance = distance.Values.Max();

        // 最大距離以上のセルを候補とし、その中からランダムにゴールを決定
        var goalCandidates = distance
            .Where(pair => pair.Value >= maxDistance)
            .Select(pair => pair.Key)
            .ToList();
        _goal = goalCandidates[UnityEngine.Random.Range(0, goalCandidates.Count)];

        // 初期の選択ノードは開始地点
        _selected = _start;
    }

    /// <summary>
    /// 現在選択中のノード(Cell)を返す。
    /// </summary>
    public Cell GetSelectedNode()
    {
        return _cells[_selected.y, _selected.x];
    }

    /// <summary>
    /// 選択ノードを指定方向へ1マス移動させる。
    /// 範囲外・壁(#)なら移動しない。移動できたらtrueを返す。
    /// </summary>
    public bool TryMoveSelected(Vector2Int direction)
    {
        var next = _selected + direction;
        if (next.x < 0 || next.x >= _currentMap[0].Length || next.y < 0 || next.y >= _currentMap.Count)
        {
            return false;
        }
        if (_currentMap[next.y][next.x] == '#')
        {
            return false;
        }

        // 移動元をEmptyへ戻して選択位置を更新する
        SetCellVisual(_selected, CellType.Empty);
        _selected = next;

        // ゴール到達なら次のページ(マップ)をロードする
        if (_selected == _goal)
        {
            OnMapCleared?.Invoke();
            LoadNextPage();
            return true;
        }

        SetCellVisual(_selected, CellType.Selected);
        return true;
    }

    /// <summary>
    /// 現在のセルを破棄し、次のマップ(ページ)を読み込んで作り直す。
    /// </summary>
    public void LoadNextPage()
    {
        LoadMap();
        DecideStartAndGoal();
        BuildCells();
    }

    /// <summary>
    /// 与えられた経路に沿って、選択ノードを duration 秒かけてゴールまで移動させる。
    /// path[0] は現在地とみなしてスキップする。ゴール到達時は内部で次ページへ切り替わる。
    /// </summary>
    public IEnumerator FollowPathRoutine(IReadOnlyList<Vector2Int> path, float duration)
    {
        if (path == null || path.Count <= 1)
        {
            yield break;
        }

        var interval = duration / (path.Count - 1);
        var goal = _goal;
        for (int i = 1; i < path.Count; i++)
        {
            var target = path[i];
            var reachingGoal = target == goal;
            TryMoveSelected(target - _selected);
            if (reachingGoal)
            {
                // ゴール到達でマップが切り替わったので終了
                yield break;
            }
            yield return new WaitForSeconds(interval);
        }
    }

    /// <summary>
    /// 指定座標のセルの見た目(Sprite)と種別を更新する。
    /// </summary>
    private void SetCellVisual(Vector2Int position, CellType type)
    {
        SetCellVisual(_cells[position.y, position.x], type);
    }

    /// <summary>
    /// 指定セルの見た目(Sprite)と種別を更新する。
    /// </summary>
    private void SetCellVisual(Cell cell, CellType type)
    {
        cell.SetImage(_cellSprites.GetValueOrDefault(type));
        cell.SetType(type);
    }

    /// <summary>
    /// 指定セルからの最短距離(歩数)をBFSで算出する。
    /// 壁(#)は通行不可、範囲外は無視する。
    /// </summary>
    private Dictionary<Vector2Int, int> BuildDistanceMap(Vector2Int start)
    {
        var distance = new Dictionary<Vector2Int, int>();
        var queue = new Queue<Vector2Int>();
        int[] dx = new int[] { 0, 0, 1, -1 };
        int[] dy = new int[] { 1, -1, 0, 0 };

        distance[start] = 0;
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                var next = new Vector2Int(current.x + dx[i], current.y + dy[i]);
                if (next.x < 0 || next.x >= _currentMap[0].Length || next.y < 0 || next.y >= _currentMap.Count)
                {
                    continue;
                }
                if (_currentMap[next.y][next.x] == '#')
                {
                    continue;
                }
                if (distance.ContainsKey(next))
                {
                    continue;
                }
                distance[next] = distance[current] + 1;
                queue.Enqueue(next);
            }
        }
        return distance;
    }

    private void Awake()
    {
        _gridDic = new Dictionary<int, List<string>>();
        if (!TrySetMaps())
        {
            Debug.LogError("マップの読み込みに失敗しました。");
        }
        _gridLayoutGroup = GetComponent<GridLayoutGroup>();
        LoadMap();
        DecideStartAndGoal();
    }
    private void Start()
    {
        BuildCells();
    }

    /// <summary>
    /// 現在のマップからセルを生成してグリッドに並べる。
    /// </summary>
    private void BuildCells()
    {
        if (_currentMap == null)
        {
            return;
        }

        var height = _currentMap.Count;
        var width = _currentMap[0].Length;
        _cells = new Cell[height, width];

        // プールのセルを使い回し、_currentMap に沿って塗り直す
        var index = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = GetOrCreateCell(index);
                cell.gameObject.SetActive(true);
                var cellType = DecideCellType(new Vector2Int(x, y), _currentMap[y][x]);
                SetCellVisual(cell, cellType);
                _cells[y, x] = cell;
                index++;
            }
        }

        // 余ったセルは Destroy せず非表示にして次ページ用に残す
        for (int i = index; i < _cellPool.Count; i++)
        {
            _cellPool[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// プールから index 番目のセルを取得する。無ければ新規生成して追加する。
    /// </summary>
    private Cell GetOrCreateCell(int index)
    {
        if (index < _cellPool.Count)
        {
            return _cellPool[index];
        }
        var cellObject = new GameObject($"Cell_{index}");
        cellObject.transform.SetParent(transform);
        var cell = cellObject.AddComponent<Cell>();
        _cellPool.Add(cell);
        return cell;
    }

    /// <summary>
    /// マップ文字と開始/ゴール地点から、そのセルの種別を決める。
    /// </summary>
    private CellType DecideCellType(Vector2Int position, char value)
    {
        CellType cellType;
        switch (value)
        {
            case '.':
                cellType = CellType.Empty;
                break;
            case '#':
                cellType = CellType.Wall;
                break;
            case 'S':
                cellType = CellType.Selected;
                break;
            case 'G':
                cellType = CellType.Goal;
                break;
            default:
                Debug.LogError($"未知のセルタイプ: {value}");
                cellType = CellType.Empty;
                break;
        }

        // 決定した開始/ゴール地点を上書き反映する
        if (position == _start)
        {
            cellType = CellType.Selected;
        }
        else if (position == _goal)
        {
            cellType = CellType.Goal;
        }
        return cellType;
    }
}
