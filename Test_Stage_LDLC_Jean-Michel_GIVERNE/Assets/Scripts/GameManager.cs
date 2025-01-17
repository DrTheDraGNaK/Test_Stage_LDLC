using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private GameObject defeatScreen;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI[] objectivesText;

    private float remainingTime;
    private bool isGameOver = false;
    private Dictionary<ColorType, int> objectivesByType = new Dictionary<ColorType, int>();
    private Dictionary<ColorType, int> currentCounts = new Dictionary<ColorType, int>();

    private void Start()
    {
        if (!ValidateSerializedFields())
        {
            Debug.LogError("GameManager: Missing references in inspector!");
            return;
        }

        InitializeGame();
        LockCursor();
    }

    private bool ValidateSerializedFields()
    {
        if (gameConfig == null || victoryScreen == null || defeatScreen == null || timerText == null || objectivesText == null || objectivesText.Length == 0)
        {
            Debug.LogError("One or more serialized fields are missing!");
            return false;
        }

        return true;
    }

    private void InitializeGame()
    {
        remainingTime = gameConfig.gameTimeInSeconds;
        isGameOver = false;

        var depositZones = FindObjectsOfType<DepositZone>();

        if (depositZones.Length == 0)
        {
            Debug.LogError("No deposit zones found in the scene!");
            return;
        }

        var zonesByType = depositZones
            .GroupBy(zone => zone.GetAcceptedType())
            .ToDictionary(g => g.Key, g => g.ToList());

        DistributeObjectives(zonesByType.Keys.ToList());

        // Initialize current counts
        foreach (var type in objectivesByType.Keys)
        {
            currentCounts[type] = 0;
        }

        foreach (var zone in depositZones)
        {
            if (!objectivesByType.ContainsKey(zone.GetAcceptedType()))
            {
                Debug.LogError($"Zone has an unknown type: {zone.GetAcceptedType()}");
                continue;
            }

            zone.SetObjectiveCount(objectivesByType[zone.GetAcceptedType()]);
        }

        UpdateObjectivesDisplay();

        // Hide screens
        victoryScreen.SetActive(false);
        defeatScreen.SetActive(false);
    }

    private void DistributeObjectives(List<ColorType> types)
    {
        objectivesByType.Clear();
        int remainingObjects = gameConfig.totalObjectsRequired;

        for (int i = 0; i < types.Count - 1; i++)
        {
            int maxForThisType = remainingObjects - (types.Count - i - 1); // Ensure at least 1 for each remaining type
            int assignedAmount = Random.Range(1, maxForThisType + 1);
            remainingObjects -= assignedAmount;
            objectivesByType[types[i]] = assignedAmount;
        }

        objectivesByType[types[types.Count - 1]] = remainingObjects;

        Debug.Log("Distributed objectives:");
        foreach (var kvp in objectivesByType)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }
    }

    private void Update()
    {
        if (isGameOver) return;

        remainingTime -= Time.deltaTime;
        UpdateTimerDisplay();

        if (remainingTime <= 0)
        {
            EndGame();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
        {
            Debug.LogError("TimerText is not assigned!");
            return;
        }

        if (remainingTime <= 0)
        {
            timerText.text = "00:00";
            return;
        }

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void UpdateObjectivesDisplay()
    {
        int index = 0;
        foreach (var objective in objectivesByType)
        {
            if (index < objectivesText.Length)
            {
                if (!currentCounts.ContainsKey(objective.Key))
                {
                    Debug.LogError($"Objective key {objective.Key} not found in currentCounts.");
                    continue;
                }

                objectivesText[index].text =
                    $"{objective.Key}: {currentCounts[objective.Key]}/{objective.Value}";
                index++;
            }
        }
    }

    public void OnObjectCountChanged(ColorType type, int newCount)
    {
        if (!objectivesByType.ContainsKey(type))
        {
            Debug.LogError($"Type {type} not found in objectivesByType.");
            return;
        }

        if (!currentCounts.ContainsKey(type))
        {
            Debug.LogError($"Type {type} not found in currentCounts.");
            return;
        }

        currentCounts[type] = Mathf.Min(newCount, objectivesByType[type]);
        UpdateObjectivesDisplay();
        CheckVictoryCondition();
    }

    private void CheckVictoryCondition()
    {
        bool allObjectivesComplete = true;
        int totalCurrentCount = 0;

        foreach (var objective in objectivesByType)
        {
            if (!currentCounts.ContainsKey(objective.Key) || currentCounts[objective.Key] != objective.Value)
            {
                allObjectivesComplete = false;
                break;
            }

            totalCurrentCount += currentCounts[objective.Key];
        }

        if (allObjectivesComplete && totalCurrentCount == gameConfig.totalObjectsRequired)
        {
            ShowVictoryScreen();
        }
    }

    private void EndGame()
    {
        isGameOver = true;
        ShowDefeatScreen();
    }

    private void ShowVictoryScreen()
    {
        if (victoryScreen == null)
        {
            Debug.LogError("Victory screen is not assigned!");
            return;
        }

        isGameOver = true;
        victoryScreen.SetActive(true);
        UnlockCursor();
    }

    private void ShowDefeatScreen()
    {
        if (defeatScreen == null)
        {
            Debug.LogError("Defeat screen is not assigned!");
            return;
        }

        defeatScreen.SetActive(true);
        UnlockCursor();

        int textIndex = 0;
        foreach (var objective in objectivesByType)
        {
            if (textIndex < objectivesText.Length)
            {
                int missing = objective.Value - currentCounts[objective.Key];
                objectivesText[textIndex].text =
                    $"{objective.Key}: {missing} remaining";
                textIndex++;
            }
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartScene()
    {
        if (victoryScreen != null) victoryScreen.SetActive(false);
        if (defeatScreen != null) defeatScreen.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
