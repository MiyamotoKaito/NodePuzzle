using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Image))]
public class Cell : MonoBehaviour
{
    public CellType CurrentCellType => _cellType;
    public void SetImage(Sprite sprite)
    {
        _image.sprite = sprite;
    }
    public void SetType(CellType type)
    {
        _cellType = type;
    }
    private Image _image;
    private CellType _cellType;
    private void Awake()
    {
        _image = GetComponent<Image>();
        _cellType = CellType.Empty;
    }
}
