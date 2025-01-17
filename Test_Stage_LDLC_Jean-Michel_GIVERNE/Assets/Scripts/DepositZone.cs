using UnityEngine;
using UnityEngine.Events;
using UnityEvent = UnityEngine.Events.UnityEvent;

public class DepositZone : MonoBehaviour
{
    [SerializeField] private ColorType acceptedType;
    [SerializeField] private int objectCount = 0;

    public UnityEvent<int> onObjectCountChanged;

    private void OnTriggerEnter(Collider other)
    {
        PickableObject item = other.GetComponent<PickableObject>();

        if (item != null)
        {
            // Check if the type matches
            if (item.GetColorType() == acceptedType)
            {
                objectCount++;
                onObjectCountChanged?.Invoke(objectCount);
                Debug.Log($"Object deposited! Total count: {objectCount}");
            }
            else
            {
                Debug.Log("Incorrect object type for this zone");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PickableObject item = other.GetComponent<PickableObject>();

        if (item != null && item.GetColorType() == acceptedType)
        {
            objectCount--;
            onObjectCountChanged?.Invoke(objectCount);
            Debug.Log($"Object removed! Total count: {objectCount}");
        }
    }

    public void SetAcceptedType(ColorType newType)
    {
        acceptedType = newType;
    }

    public int GetObjectCount()
    {
        return objectCount;
    }
}