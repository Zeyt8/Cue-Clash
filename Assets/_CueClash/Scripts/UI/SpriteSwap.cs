using UnityEngine;
using UnityEngine.UI;

public class SpriteSwap : MonoBehaviour
{
    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;

    private bool isSprite1 = true;
    private Image image;

    public void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Swap()
    {
        if (isSprite1)
        {
            image.sprite = sprite2;
            isSprite1 = false;
        }
        else
        {
            image.sprite = sprite1;
            isSprite1 = true;
        }
    }
}
