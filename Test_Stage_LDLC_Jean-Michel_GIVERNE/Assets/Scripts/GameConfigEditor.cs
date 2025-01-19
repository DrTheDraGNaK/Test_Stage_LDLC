using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameConfig))]
public class GameConfigEditor : Editor
{
    private SerializedProperty gameTimeInSeconds;
    private SerializedProperty totalObjectsRequired;

    private bool timeSettingsFoldout = true;
    private bool objectSettingsFoldout = true;

    private const int MIN_OBJECTS = 4;
    private const int MAX_OBJECTS = 50;

    private void OnEnable()
    {
        gameTimeInSeconds = serializedObject.FindProperty("gameTimeInSeconds");
        totalObjectsRequired = serializedObject.FindProperty("totalObjectsRequired");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);

        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 16;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Game Configuration", headerStyle);

        EditorGUILayout.Space(15);

        DrawTimerSection();

        EditorGUILayout.Space(10);

        DrawObjectSection();

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void DrawTimerSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        timeSettingsFoldout = EditorGUILayout.Foldout(timeSettingsFoldout, "Timer Settings", true);

        if (timeSettingsFoldout)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            float minutes = Mathf.Floor(gameTimeInSeconds.floatValue / 60);
            float seconds = gameTimeInSeconds.floatValue % 60;

            EditorGUILayout.LabelField("Game Duration", GUILayout.Width(100));

            EditorGUI.BeginChangeCheck();
            minutes = EditorGUILayout.FloatField(minutes, GUILayout.Width(50));
            EditorGUILayout.LabelField("min", GUILayout.Width(30));
            seconds = EditorGUILayout.FloatField(seconds, GUILayout.Width(50));
            EditorGUILayout.LabelField("sec", GUILayout.Width(30));

            if (EditorGUI.EndChangeCheck())
            {
                gameTimeInSeconds.floatValue = (minutes * 60) + seconds;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"Total time in seconds: {gameTimeInSeconds.floatValue}");

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawObjectSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        objectSettingsFoldout = EditorGUILayout.Foldout(objectSettingsFoldout, "Object Settings", true);

        if (objectSettingsFoldout)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Required Objects ({MIN_OBJECTS}-{MAX_OBJECTS}):");

            EditorGUI.BeginChangeCheck();
            int newValue = EditorGUILayout.IntField(totalObjectsRequired.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                totalObjectsRequired.intValue = Mathf.Clamp(newValue, MIN_OBJECTS, MAX_OBJECTS);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUI.ProgressBar(
                EditorGUILayout.GetControlRect(false, 20),
                (float)(totalObjectsRequired.intValue - MIN_OBJECTS) / (MAX_OBJECTS - MIN_OBJECTS),
                $"Objects: {totalObjectsRequired.intValue}/{MAX_OBJECTS}"
            );

            if (totalObjectsRequired.intValue < MIN_OBJECTS || totalObjectsRequired.intValue > MAX_OBJECTS)
            {
                EditorGUILayout.HelpBox($"Le nombre d'objets doit être entre {MIN_OBJECTS} et {MAX_OBJECTS}.", MessageType.Warning);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
    }
}