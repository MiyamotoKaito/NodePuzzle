using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 時間経過とマップクリアで溜まるゲージ。
/// 満タン時に全消費すると自動探索(オートクリア)を発動できる。
/// 大まかに4クリア + 経過時間で満タンになる想定。
/// </summary>
[RequireComponent(typeof(Image))]
public class Gauge : MonoBehaviour
{
    [SerializeField] private float _fillPerSecond = 0.02f; // 時間経過で溜まる速度(毎秒)
    [SerializeField] private float _fillPerClear = 0.2f;   // 1クリアで溜まる量(4クリア=0.8、残りは時間で補完)

    private Image _gaugeImage;
    private float _value;

    /// <summary>ゲージが満タンかどうか。</summary>
    public bool IsFull => _value >= 1f;

    private void Awake()
    {
        _gaugeImage = GetComponent<Image>();
    }

    private void Update()
    {
        // 時間経過で加算
        AddAmount(_fillPerSecond * Time.deltaTime);
    }

    /// <summary>マップクリア時に呼び、クリア分を加算する。</summary>
    public void AddClearAmount()
    {
        AddAmount(_fillPerClear);
    }

    /// <summary>ゲージを全消費する。</summary>
    public void Consume()
    {
        _value = 0f;
        ApplyToImage();
    }

    private void AddAmount(float amount)
    {
        _value = Mathf.Clamp01(_value + amount);
        ApplyToImage();
    }

    private void ApplyToImage()
    {
        _gaugeImage.fillAmount = _value;
    }
}
