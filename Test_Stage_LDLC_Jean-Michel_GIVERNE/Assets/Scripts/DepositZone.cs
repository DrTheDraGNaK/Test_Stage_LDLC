using UnityEngine;

public class DepositZone : MonoBehaviour
{
    [SerializeField] private ColorType acceptedType;
    [SerializeField] private int objectCount = 0;
    [SerializeField] private GameManager gameManager;

    private int objectiveCount;

    private void OnTriggerEnter(Collider other)
    {
        PickableObject item = other.GetComponent<PickableObject>();

        if (item != null && item.GetColorType() == acceptedType)
        {
            objectCount++;
            gameManager.OnObjectCountChanged(acceptedType, objectCount);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PickableObject item = other.GetComponent<PickableObject>();

        if (item != null && item.GetColorType() == acceptedType)
        {
            objectCount--;
            gameManager.OnObjectCountChanged(acceptedType, objectCount);
        }
    }

    public ColorType GetAcceptedType()
    {
        return acceptedType;
    }

    public void SetObjectiveCount(int count)
    {
        objectiveCount = count;
    }
}