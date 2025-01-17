using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PaintBall : MonoBehaviour
{
    private ColorType color;

    public void Initialize(ColorType paintColor)
    {
        color = paintColor;
        GetComponent<Renderer>().material.color = GetColorFromType(paintColor);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;
        PickableObject pickable = hitObject.GetComponent<PickableObject>();

        if (pickable != null)
        {
            pickable.SetColorType(color);
        }

        Destroy(gameObject); 
    }

    private Color GetColorFromType(ColorType colorType)
    {
        switch (colorType)
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