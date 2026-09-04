using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

/// <summary>
/// WASD入力で選択ノードを移動させる。
/// 入力Vector2から方向を決めるだけで、実際の移動判定・見た目更新はTableに委譲する。
/// Jumpキーはゲージ発動(自動探索)のトリガーとしてイベント通知する。
/// </summary>
public class PlayerController : MonoBehaviour
{
    private Table _table;
    private InputSystem_Actions _actions;

    /// <summary>trueの間、手動移動入力を無視する(自動探索中など)。</summary>
    public bool InputLocked { get; set; }
    /// <summary>発動キー(Jump)が押されたときに発火する。</summary>
    public event Action OnActivate;

    [Inject]
    public void Construct(Table table)
    {
        _table = table;
    }

    private void Awake()
    {
        _actions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _actions.Player.Enable();
        _actions.Player.Move.performed += OnMove;
        _actions.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        _actions.Player.Move.performed -= OnMove;
        _actions.Player.Jump.performed -= OnJump;
        _actions.Player.Disable();
    }

    private void OnDestroy()
    {
        _actions.Dispose();
    }

    /// <summary>
    /// Move入力(Vector2)を上下左右いずれか1方向へ量子化し、その方向へ選択ノードの移動をTableに要求する。
    /// </summary>
    private void OnMove(InputAction.CallbackContext context)
    {
        if (InputLocked)
        {
            return;
        }
        var direction = ToDirection(context.ReadValue<Vector2>());
        if (direction == Vector2Int.zero)
        {
            return;
        }
        _table.TryMoveSelected(direction);
    }

    /// <summary>
    /// 発動キー入力。実際の発動可否(ゲージ満タン判定など)は購読側に委ねる。
    /// </summary>
    private void OnJump(InputAction.CallbackContext context)
    {
        OnActivate?.Invoke();
    }

    /// <summary>
    /// 入力ベクトルをグリッド座標系(y下向き)の1方向Vector2Intへ変換する。
    /// 斜め入力は成分が大きい方を優先する。
    /// 画面上方向(input.y > 0)は行インデックスが減る向きなのでyを反転させている。
    /// </summary>
    private Vector2Int ToDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.25f)
        {
            return Vector2Int.zero;
        }
        if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
        {
            return input.x > 0 ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
        }
        return input.y > 0 ? new Vector2Int(0, -1) : new Vector2Int(0, 1);
    }
}
