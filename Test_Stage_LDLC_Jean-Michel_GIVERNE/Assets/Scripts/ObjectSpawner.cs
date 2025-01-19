using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private PickableObject pickablePrefab;
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(10f, 10f);
    [SerializeField] private float spawnHeight = 0.5f;

    [Header("Organization Settings")]
    [SerializeField] private string containerName = "Pickable Objects";

    private Transform objectsContainer;
    private List<ColorType> availableColors;

    private void Start()
    {
        InitializeSpawner();
        SpawnObjects();
    }

    private void InitializeSpawner()
    {
        GameObject container = GameObject.Find(containerName);
        if (container == null)
        {
            container = new GameObject(containerName);
        }
        objectsContainer = container.transform;

        availableColors = new List<ColorType>();
        foreach (ColorType color in System.Enum.GetValues(typeof(ColorType)))
        {
            if (color != ColorType.None)
            {
                availableColors.Add(color);
            }
        }
    }

    private void SpawnObjects()
    {
        for (int i = 0; i < gameConfig.totalObjectsRequired; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            PickableObject newObject = Instantiate(pickablePrefab, spawnPosition, Random.rotation, objectsContainer);

            ColorType randomColor = availableColors[Random.Range(0, availableColors.Count)];
            newObject.SetColorType(randomColor);

            newObject.gameObject.name = $"PickableObject_{randomColor}_{i}";
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float halfWidth = spawnAreaSize.x * 0.5f;
        float halfLength = spawnAreaSize.y * 0.5f;

        Vector3 spawnerPosition = transform.position;

        float x = spawnerPosition.x + Random.Range(-halfWidth, halfWidth);
        float z = spawnerPosition.z + Random.Range(-halfLength, halfLength);

        return new Vector3(x, spawnerPosition.y + spawnHeight, z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3(transform.position.x, spawnHeight, transform.position.z),
            new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y)
        );
    }
}