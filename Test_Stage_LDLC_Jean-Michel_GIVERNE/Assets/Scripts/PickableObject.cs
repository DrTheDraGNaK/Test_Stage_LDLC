using UnityEngine;

public enum ColorType
{
    None,
    Red,
    Blue,
    Green,
    Yellow
}

public class PickableObject : MonoBehaviour
{
    [SerializeField] private ColorType colorType = ColorType.None;
    private Renderer objectRenderer;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        UpdateVisualColor();
    }

    public ColorType GetColorType()
    {
        return colorType;
    }

    public void SetColorType(ColorType newColor)
    {
        colorType = newColor;
        UpdateVisualColor();
    }

    private void UpdateVisualColor()
    {
        if (objectRenderer == null) return;

        Color newColor = GetColorFromType(colorType);
        objectRenderer.material.color = newColor;
    }

    private Color GetColorFromType(ColorType type)
    {
        switch (type)
        {
            case ColorType.None:
                return Color.white; 
            case ColorType.Red:
                return Color.red;
            case ColorType.Blue:
                return Color.blue;
            case ColorType.Green:
                return Color.green;
            case ColorType.Yellow:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
}