using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Scriptable Objects/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Timer Settings")]
    public float gameTimeInSeconds = 300f;

    [Header("Object Settings")]
    public int totalObjectsRequired = 15;
}
