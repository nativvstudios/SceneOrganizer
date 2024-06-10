using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor.SceneManagement;

public class SceneOrganizerWindow : EditorWindow
{
    private List<string> scenes = new List<string>();
    private Dictionary<string, List<string>> sceneGroups = new Dictionary<string, List<string>>();
    private string newGroupName = "";
    private string savePath = "Assets/Editor/SceneGroups.dat";
    private Vector2 mainScrollPosition;
    private Vector2 sceneScrollPosition;
    private Vector2 groupScrollPosition;
    private float sceneAreaHeight = 200;
    private bool isResizing = false;
    private string draggingScene;
    private string targetGroup;
    private string searchQuery = "";
    private string selectedScene;
    private string renameScene = null;
    private string newSceneName = "";
    private double lastClickTime;
    private const double doubleClickThreshold = 0.3;
    private bool isGroupScrollViewActive = false;

    [MenuItem("Window/Scene Organizer")]
    public static void ShowWindow()
    {
        GetWindow<SceneOrganizerWindow>("Scene Organizer");
    }

    private void OnEnable()
    {
        LoadScenes();
        LoadGroups();
        EnsureSavePathExists();
    }

    private void OnDisable()
    {
        SaveGroups();
    }

    private void EnsureSavePathExists()
    {
        string directoryPath = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
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
        scenes.Sort();
    }

    private void SaveGroups()
    {
        EnsureSavePathExists();
        try
        {
            using (FileStream file = File.Open(savePath, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, sceneGroups);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save scene groups: {e.Message}");
        }
    }

    private void LoadGroups()
    {
        if (File.Exists(savePath))
        {
            try
            {
                using (FileStream file = File.Open(savePath, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    sceneGroups = (Dictionary<string, List<string>>)bf.Deserialize(file);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load scene groups: {e.Message}");
            }
        }
    }

    private void OnGUI()
    {
        float totalHeight = position.height - 50; // Adjust based on other elements in your window
        sceneAreaHeight = Mathf.Clamp(totalHeight * 0.5f, 100, totalHeight - 100); // Allocate half the height to scenes, but keep minimum and maximum limits

        mainScrollPosition = GUILayout.BeginScrollView(mainScrollPosition);

        GUILayout.Label("Scenes in Project", EditorStyles.boldLabel);

        DrawSearchBar();
        DrawScenesList();

        HandleDragAndDrop();

        GUILayout.Space(5);
        DrawResizeHandle();
        DrawAddSceneToGroupButton();

        GUILayout.Label("Manage Groups", EditorStyles.boldLabel);

        DrawNewGroupSection();
        GUILayout.Space(10);
        DrawGroupSection();

        GUILayout.EndScrollView();

        // Draw watermark only if the group scroll view is not active
        if (!isGroupScrollViewActive)
        {
            DrawWatermark();
        }
    }

    private void DrawSearchBar()
    {
        GUILayout.BeginHorizontal();
        searchQuery = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField);
        GUILayout.EndHorizontal();
    }

    private void DrawScenesList()
    {
        sceneScrollPosition = GUILayout.BeginScrollView(sceneScrollPosition, GUILayout.Height(sceneAreaHeight));

        foreach (string scene in scenes)
        {
            if (!string.IsNullOrEmpty(searchQuery) && !Path.GetFileNameWithoutExtension(scene).ToLower().Contains(searchQuery.ToLower()))
            {
                continue;
            }

            DrawSceneItem(scene);
        }

        GUILayout.EndScrollView();
    }

    private void DrawSceneItem(string scene)
    {
        GUILayout.BeginHorizontal();

        GUIStyle sceneStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fixedHeight = 20,
            margin = new RectOffset(4, 4, 2, 2),
            alignment = TextAnchor.MiddleLeft
        };

        if (scene == renameScene)
        {
            DrawRenameSceneField(scene);
        }
        else
        {
            DrawSceneLabel(scene, sceneStyle);
            if (GUILayout.Button("Rename", GUILayout.Width(60)))
            {
                StartRenamingScene(scene);
            }
        }

        GUILayout.EndHorizontal();
    }

    private void DrawRenameSceneField(string scene)
    {
        newSceneName = GUILayout.TextField(newSceneName, GUILayout.Width(150));
        if (GUILayout.Button("Rename", GUILayout.Width(60)))
        {
            RenameScene(scene, newSceneName);
            renameScene = null;
        }
        if (GUILayout.Button("Cancel", GUILayout.Width(60)))
        {
            renameScene = null;
        }
    }

    private void DrawSceneLabel(string scene, GUIStyle sceneStyle)
    {
        if (scene == selectedScene)
        {
            HighlightSelectedScene(scene, sceneStyle);
        }
        else
        {
            GUILayout.Label(Path.GetFileNameWithoutExtension(scene), sceneStyle);
        }

        HandleSceneSelection(scene);
    }

    private void HighlightSelectedScene(string scene, GUIStyle sceneStyle)
    {
        sceneStyle.normal.textColor = Color.white;
        GUI.backgroundColor = Color.blue;
        GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(20));
        GUI.backgroundColor = Color.white;
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUI.Label(lastRect, Path.GetFileNameWithoutExtension(scene), sceneStyle);
    }

    private void HandleSceneSelection(string scene)
    {
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
                DragAndDrop.objectReferences = new UnityEngine.Object[0];
                DragAndDrop.StartDrag("DraggingScene");
                Event.current.Use();
                Repaint();
            }
            lastClickTime = EditorApplication.timeSinceStartup;
        }
    }

    private void StartRenamingScene(string scene)
    {
        renameScene = scene;
        newSceneName = Path.GetFileNameWithoutExtension(scene);
    }

    private void HandleDragAndDrop()
    {
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
    }

    private void DrawResizeHandle()
    {
        Rect handleRect = GUILayoutUtility.GetLastRect();
        handleRect.height = 5;
        EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeVertical);

        if (Event.current.type == EventType.MouseDown && handleRect.Contains(Event.current.mousePosition))
        {
            isResizing = true;
        }

        if (isResizing)
        {
            float newHeight = sceneAreaHeight + Event.current.delta.y;
            sceneAreaHeight = Mathf.Clamp(newHeight, 100, position.height - 150);
            Repaint();
        }

        if (Event.current.type == EventType.MouseUp)
        {
            isResizing = false;
        }
    }

    private void DrawAddSceneToGroupButton()
    {
        if (!string.IsNullOrEmpty(selectedScene))
        {
            if (GUILayout.Button("Add Selected Scene to Group", EditorStyles.miniButton))
            {
                if (sceneGroups.Count == 0)
                {
                    EditorUtility.DisplayDialog("No Groups", "You must have at least one group to add the scene to.", "OK");
                    return;
                }

                GenericMenu menu = new GenericMenu();
                foreach (var group in sceneGroups.Keys)
                {
                    string groupName = group;
                    menu.AddItem(new GUIContent(group), false, () => AddSceneToGroup(selectedScene, groupName));
                }
                menu.ShowAsContext();
            }
        }
    }

    private void DrawNewGroupSection()
    {
        EditorGUILayout.BeginHorizontal();
        newGroupName = EditorGUILayout.TextField("New Group Name", newGroupName, GUILayout.ExpandWidth(true));

        if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && !string.IsNullOrEmpty(newGroupName))
        {
            AddNewGroup();
            Event.current.Use();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Add Group", EditorStyles.miniButton))
        {
            AddNewGroup();
        }
    }

    private void DrawGroupSection()
    {
        float remainingHeight = position.height - sceneAreaHeight - 110; // Adjust 110 based on other elements in your window and leave room for watermark
        remainingHeight = Mathf.Max(remainingHeight, 100); // Ensure a minimum height

        groupScrollPosition = GUILayout.BeginScrollView(groupScrollPosition, GUILayout.Height(remainingHeight));
        isGroupScrollViewActive = groupScrollPosition.y > 0; // Check if the group scroll view is active

        foreach (var group in new Dictionary<string, List<string>>(sceneGroups))
        {
            GUILayout.BeginVertical();
            GUILayout.Label(group.Key, EditorStyles.boldLabel);
            Rect groupRect = GUILayoutUtility.GetLastRect();
            if (draggingScene != null && groupRect.Contains(Event.current.mousePosition))
            {
                targetGroup = group.Key;
                Repaint();
            }

            foreach (var scene in group.Value)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("  - " + Path.GetFileNameWithoutExtension(scene), EditorStyles.label);

                if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    OpenScene(scene);
                }

                if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(70)))
                {
                    group.Value.Remove(scene);
                    break;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove Group", EditorStyles.miniButton))
            {
                if (EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete the group '{group.Key}'?", "Yes", "No"))
                {
                    sceneGroups.Remove(group.Key);
                    break;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        GUILayout.EndScrollView();
    }

    private void RenameScene(string oldScenePath, string newSceneName)
    {
        string newPath = Path.Combine(Path.GetDirectoryName(oldScenePath), newSceneName + ".unity");
        AssetDatabase.RenameAsset(oldScenePath, newSceneName);
        AssetDatabase.SaveAssets();
        LoadScenes();
    }

    private void AddNewGroup()
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

    private void DrawWatermark()
    {
        GUIStyle watermarkStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.LowerRight,
            fontSize = 10,
            normal = { textColor = new Color(0.5f, 0.5f, 0.5f, 0.8f) } // Adjust color to be darker than the dark editor color
        };

        Rect watermarkRect = new Rect(position.width - 150, position.height - 20, 140, 20);
        GUI.Label(watermarkRect, "Made by Nativvstudios", watermarkStyle);
    }
}
