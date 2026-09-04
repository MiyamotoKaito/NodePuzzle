using System.Collections;
using UnityEngine;
using VContainer;

/// <summary>
/// ゲージ満タン時の手動発動を受けて、A*(IAlgorithm)で現在地→ゴールの経路を探索し、
/// 選択ノードを一定時間かけて自動でゴールまで移動させる。
/// またマップクリアを検知してゲージを溜める。
/// </summary>
public class AutoSolveController : MonoBehaviour
{
    private const float AutoSolveDuration = 2f;

    private Table _table;
    private Gauge _gauge;
    private IAlgorithm _algorithm;
    private PlayerController _playerController;
    private bool _isSolving;

    [Inject]
    public void Construct(Table table, Gauge gauge, IAlgorithm algorithm, PlayerController playerController)
    {
        _table = table;
        _gauge = gauge;
        _algorithm = algorithm;
        _playerController = playerController;
    }

    // 注入完了後(Start以降)にイベント購読する
    private void Start()
    {
        _table.OnMapCleared += OnMapCleared;
        _playerController.OnActivate += OnActivate;
    }

    private void OnDestroy()
    {
        if (_table != null)
        {
            _table.OnMapCleared -= OnMapCleared;
        }
        if (_playerController != null)
        {
            _playerController.OnActivate -= OnActivate;
        }
    }

    // マップクリアでゲージを溜める
    private void OnMapCleared()
    {
        _gauge.AddClearAmount();
    }

    // 発動キー: ゲージが満タンなら全消費して自動探索を開始する
    private void OnActivate()
    {
        if (_isSolving || !_gauge.IsFull)
        {
            return;
        }
        _gauge.Consume();
        StartCoroutine(AutoSolveRoutine());
    }

    private IEnumerator AutoSolveRoutine()
    {
        _isSolving = true;
        _playerController.InputLocked = true;

        var path = _algorithm.Search(_table.SelectedPosition, _table.GoalPosition, _table.CurrentMap);
        yield return _table.FollowPathRoutine(path, AutoSolveDuration);

        _playerController.InputLocked = false;
        _isSolving = false;
    }
}
