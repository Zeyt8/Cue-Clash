using UnityEngine;
using UnityEngine.UI;

public class SpriteSwap : MonoBehaviour
{
    [SerializeField] private Sprite _sprite1;
    [SerializeField] private Sprite _sprite2;

    private bool _isSprite1 = true;
    private Image _image;

    public void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void Swap()
    {
        if (_isSprite1)
        {
            _image.sprite = _sprite2;
            _isSprite1 = false;
        }
        else
        {
            _image.sprite = _sprite1;
            _isSprite1 = true;
        }
    }
}
