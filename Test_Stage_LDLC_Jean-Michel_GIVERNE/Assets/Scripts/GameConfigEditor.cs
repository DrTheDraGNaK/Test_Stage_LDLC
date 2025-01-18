//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(GameConfig))]
//public class GameConfigEditor : Editor
//{
//    private GUIStyle headerStyle;
//    private GUIStyle subHeaderStyle;

//    private void InitializeStyles()
//    {
//        if (headerStyle == null)
//        {
//            headerStyle = new GUIStyle(EditorStyles.boldLabel)
//            {
//                fontSize = 14,
//                padding = new RectOffset(5, 5, 10, 10)
//            };
//        }

//        if (subHeaderStyle == null)
//        {
//            subHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
//            {
//                fontSize = 12,
//                padding = new RectOffset(5, 5, 5, 5)
//            };
//        }
//    }

//    public override void OnInspectorGUI()
//    {
//        InitializeStyles();

//        GameConfig gameConfig = (GameConfig)target;

//        EditorGUILayout.Space(10);
//        EditorGUILayout.LabelField("Game Configuration", headerStyle);

//        EditorGUILayout.Space(10);
//        EditorGUILayout.LabelField("Timer Settings", subHeaderStyle);
//        EditorGUI.indentLevel++;

//        // Timer configuration
//        EditorGUILayout.BeginHorizontal();
//        int minutes = Mathf.FloorToInt(gameConfig.gameTimeInSeconds / 60);
//        int seconds = Mathf.FloorToInt(gameConfig.gameTimeInSeconds % 60);

//        EditorGUILayout.LabelField("Game Duration:", GUILayout.Width(100));
//        minutes = EditorGUILayout.IntField("Minutes", minutes, GUILayout.Width(100));
//        seconds = EditorGUILayout.IntField("Seconds", seconds, GUILayout.Width(100));
//        EditorGUILayout.EndHorizontal();

//        gameConfig.gameTimeInSeconds = minutes * 60 + seconds;

//        EditorGUI.indentLevel--;
//        EditorGUILayout.Space(10);

//        // Objects configuration
//        EditorGUILayout.LabelField("Objects Settings", subHeaderStyle);
//        EditorGUI.indentLevel++;

//        gameConfig.totalObjectsRequired = EditorGUILayout.IntField("Total Objects Required", gameConfig.totalObjectsRequired);

//        EditorGUI.indentLevel--;

//        EditorGUILayout.Space(10);
//        EditorGUILayout.HelpBox(
//            "The total objects will be randomly distributed among the deposit zones found in the scene at runtime.",
//            MessageType.Info);

//        if (GUI.changed)
//        {
//            EditorUtility.SetDirty(target);
//        }
//    }
//}