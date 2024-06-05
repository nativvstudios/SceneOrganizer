using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor.SceneManagement;

public class SceneOrganizerWindow : EditorWindow
{
    private List<string> scenes = new List<string>();
    private Dictionary<string, List<string>> sceneGroups = new Dictionary<string, List<string>>();
    private string newGroupName = "";
    private string savePath = "Assets/Editor/SceneGroups.dat";
    private Vector2 scrollPosition;
    private float scrollAreaHeight = 200;
    private bool isResizing = false;
    private string draggingScene;
    private string targetGroup;
    private string searchQuery = "";
    private string selectedScene;
    private double lastClickTime;
    private const double doubleClickThreshold = 0.3;

    [MenuItem("Window/Scene Organizer")]
    public static void ShowWindow()
    {
        GetWindow<SceneOrganizerWindow>("Scene Organizer");
    }

    private void OnEnable()
    {
        LoadScenes();
        LoadGroups();
    }

    private void OnDisable()
    {
        SaveGroups();
    }

    private void LoadScenes()
    {
        scenes.Clear();
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in sceneGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            scenes.Add(path);
        }
        scenes.Sort(); // Sort scenes alphabetically
    }

    private void SaveGroups()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(savePath);
        bf.Serialize(file, sceneGroups);
        file.Close();
    }

    private void LoadGroups()
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(savePath, FileMode.Open);
            sceneGroups = (Dictionary<string, List<string>>)bf.Deserialize(file);
            file.Close();
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Scenes in Project", EditorStyles.boldLabel);

        // Search bar
        searchQuery = EditorGUILayout.TextField("Search Scenes", searchQuery);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(scrollAreaHeight));

        foreach (string scene in scenes)
        {
            if (!string.IsNullOrEmpty(searchQuery) && !Path.GetFileNameWithoutExtension(scene).ToLower().Contains(searchQuery.ToLower()))
            {
                continue; // Skip scenes that don't match the search query
            }

            GUILayout.BeginHorizontal();

            GUIStyle sceneStyle = new GUIStyle(GUI.skin.label);
            sceneStyle.fontSize = 12; // Adjust font size for scene labels
            sceneStyle.fixedHeight = 20; // Adjust fixed height for scene labels
            sceneStyle.margin = new RectOffset(4, 4, 2, 2); // Adjust margin around scene labels
            sceneStyle.alignment = TextAnchor.MiddleLeft; // Align text to the left

            if (scene == selectedScene)
            {
                sceneStyle.normal.textColor = Color.white;
                GUI.backgroundColor = Color.blue;
                GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(20));
                GUI.backgroundColor = Color.white;
                Rect lastRect = GUILayoutUtility.GetLastRect();
                GUI.Label(lastRect, Path.GetFileNameWithoutExtension(scene), sceneStyle);
            }
            else
            {
                GUILayout.Label(Path.GetFileNameWithoutExtension(scene), sceneStyle);
            }

            Rect labelRect = GUILayoutUtility.GetLastRect();

            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
            {
                if (scene == selectedScene && (EditorApplication.timeSinceStartup - lastClickTime) < doubleClickThreshold)
                {
                    OpenScene(scene);
                }
                else
                {
                    selectedScene = scene;
                    draggingScene = scene;
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new Object[0];
                    DragAndDrop.StartDrag("DraggingScene");
                    Event.current.Use();
                    Repaint();
                }
                lastClickTime = EditorApplication.timeSinceStartup;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        if (Event.current.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            Event.current.Use();
        }

        if (Event.current.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            if (targetGroup != null)
            {
                AddSceneToGroup(draggingScene, targetGroup);
                draggingScene = null;
                targetGroup = null;
            }
            Event.current.Use();
        }

        if (Event.current.type == EventType.MouseUp)
        {
            draggingScene = null;
        }

        GUILayout.Space(5);
        Rect handleRect = GUILayoutUtility.GetLastRect();
        handleRect.height = 5;
        EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeVertical);

        if (Event.current.type == EventType.MouseDown && handleRect.Contains(Event.current.mousePosition))
        {
            isResizing = true;
        }

        if (isResizing)
        {
            scrollAreaHeight += Event.current.delta.y;
            scrollAreaHeight = Mathf.Clamp(scrollAreaHeight, 100, position.height - 100);
            Repaint();
        }

        if (Event.current.type == EventType.MouseUp)
        {
            isResizing = false;
        }

        GUILayout.Space(10);

        if (!string.IsNullOrEmpty(selectedScene))
        {
            if (GUILayout.Button("Add Selected Scene to Group"))
            {
                if (sceneGroups.Count == 0)
                {
                    EditorUtility.DisplayDialog("No Groups", "You must have at least one group to add the scene to.", "OK");
                    return;
                }

                GenericMenu menu = new GenericMenu();
                foreach (var group in sceneGroups.Keys)
                {
                    string groupName = group; // To capture the correct value in the closure
                    menu.AddItem(new GUIContent(group), false, () => AddSceneToGroup(selectedScene, groupName));
                }
                menu.ShowAsContext();
            }
        }

        GUILayout.Label("Manage Groups", EditorStyles.boldLabel);

        newGroupName = EditorGUILayout.TextField("New Group Name", newGroupName);

        if (GUILayout.Button("Add Group"))
        {
            if (string.IsNullOrEmpty(newGroupName))
            {
                EditorUtility.DisplayDialog("Group Name Required", "Group must have a name.", "OK");
                return;
            }

            if (!sceneGroups.ContainsKey(newGroupName))
            {
                sceneGroups.Add(newGroupName, new List<string>());
                newGroupName = "";
            }
        }

        GUILayout.Space(10);

        foreach (var group in sceneGroups.ToList())
        {
            GUILayout.Label(group.Key, EditorStyles.boldLabel);
            Rect groupRect = GUILayoutUtility.GetLastRect();
            if (draggingScene != null && groupRect.Contains(Event.current.mousePosition))
            {
                targetGroup = group.Key;
                Repaint();
            }

            foreach (var scene in group.Value.ToList())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("  - " + Path.GetFileNameWithoutExtension(scene));

                if (GUILayout.Button("Open", GUILayout.Width(60)))
                {
                    OpenScene(scene);
                }

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    group.Value.Remove(scene);
                    break;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove Group"))
            {
                if (EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete the group '{group.Key}'?", "Yes", "No"))
                {
                    sceneGroups.Remove(group.Key);
                    break;
                }
            }
            GUILayout.EndHorizontal();
        }

        if (draggingScene != null && targetGroup != null && Event.current.type == EventType.DragPerform)
        {
            AddSceneToGroup(draggingScene, targetGroup);
            draggingScene = null;
            targetGroup = null;
            Event.current.Use();
        }
    }

    private void AddSceneToGroup(string scene, string group)
    {
        if (sceneGroups.ContainsKey(group) && !sceneGroups[group].Contains(scene))
        {
            sceneGroups[group].Add(scene);
        }
    }

    private void OpenScene(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }
}
