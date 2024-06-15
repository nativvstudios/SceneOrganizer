using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.SceneManagement;

public class CreateNewSceneWindow : EditorWindow
{
    private string sceneName = "NewScene";
    private string[] sceneTemplates = { "Default", "Empty" }; // Add more templates as needed
    private int selectedTemplateIndex = 0;
    private string savePath = "Assets/";

    private SceneOrganizerWindow organizerWindow;

    public static void ShowWindow(SceneOrganizerWindow organizerWindow)
    {
        CreateNewSceneWindow window = GetWindow<CreateNewSceneWindow>("Create New Scene");
        window.organizerWindow = organizerWindow;
        window.minSize = new Vector2(400, 200);
    }

    private void OnGUI()
    {
        GUILayout.Label("Create New Scene", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Scene Name:");
        sceneName = EditorGUILayout.TextField(sceneName);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Template:");
        selectedTemplateIndex = EditorGUILayout.Popup(selectedTemplateIndex, sceneTemplates);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Save Path:");
        EditorGUILayout.BeginHorizontal();
        savePath = EditorGUILayout.TextField(savePath);
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string path = EditorUtility.SaveFolderPanel("Select Folder", savePath, "");
            if (!string.IsNullOrEmpty(path))
            {
                savePath = "Assets" + path.Replace(Application.dataPath, "");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Scene"))
        {
            CreateScene();
        }
    }

    private void CreateScene()
    {
        string fullPath = Path.Combine(savePath, sceneName + ".unity");
        if (File.Exists(fullPath))
        {
            if (!EditorUtility.DisplayDialog("Overwrite Scene", "A scene with this name already exists. Do you want to overwrite it?", "Yes", "No"))
            {
                return;
            }
        }

        NewSceneSetup setup = selectedTemplateIndex == 0 ? NewSceneSetup.DefaultGameObjects : NewSceneSetup.EmptyScene;
        Scene newScene = EditorSceneManager.NewScene(setup);
        EditorSceneManager.SaveScene(newScene, fullPath);
        AssetDatabase.Refresh();

        organizerWindow?.LoadScenes(); // Refresh the scene list in SceneOrganizerWindow

        Close();
    }
}
