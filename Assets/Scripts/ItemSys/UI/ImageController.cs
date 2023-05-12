using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ImageController : MonoBehaviour
{
    static Color dimColor = new Color(1, 1, 1, .5f);
    static Color transparentColor = new Color(1, 1, 1, 0);

    [SerializeField] protected Image image;
    [SerializeField] protected Transform myTransform;

    public void SetSprite(Sprite sprite)
    {
        this.image.sprite = sprite;
    }

    public Sprite GetSprite()
    {
        return this.image.sprite;
    }

    public void SetPosition(Vector3 worldPosition)
    {
        transform.position = worldPosition;
    }

    public void Show()
    {
        image.gameObject.SetActive(true);
    }

    public void Hide()
    {
        image.gameObject.SetActive(false);
    }

    public void Brighten()
    {
        image.color = Color.white;
    }

    public void Dim()
    {
        image.color = dimColor;
    }

    public void Transparent()
    {
        image.color = transparentColor;
    }

}
