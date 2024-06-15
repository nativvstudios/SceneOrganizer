using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class SceneOrganizerWindow : EditorWindow
{
    private List<string> scenes = new List<string>();
    private SceneGroupData sceneGroupData;
    private string newGroupName = "";
    private Vector2 mainScrollPosition;
    private Vector2 sceneScrollPosition;
    private Vector2 groupScrollPosition;
    private Vector2 initialSceneScrollPosition;
    private float sceneAreaHeight = 200;
    private bool isResizing = false;
    private string draggingScene;
    private string targetGroup;
    private string searchQuery = "";
    private string selectedScene;
    private string renameScene = null;
    private string newSceneName = "";
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;
    private bool isGroupScrollViewActive = false;
    private Dictionary<string, bool> groupCollapsedStates = new Dictionary<string, bool>();

    private Vector2 initialMousePosition;
    private const float dragThreshold = 10.0f; // Threshold to start a drag

    private const string assetPath = "Assets/Editor/SceneGroupData.asset";
    private const int maxGroupNameLength = 50;

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

    public void LoadScenes()
    {
        scenes.Clear();
        string[] sceneGUIDs = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in sceneGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            scenes.Add(path);
        }
        scenes.Sort();
        Repaint(); // Ensure the window is repainted to reflect the updated list
    }

    private void SaveGroups()
    {
        EditorUtility.SetDirty(sceneGroupData);
        AssetDatabase.SaveAssets();
    }

    private void LoadGroups()
    {
        sceneGroupData = AssetDatabase.LoadAssetAtPath<SceneGroupData>(assetPath);

        if (sceneGroupData == null)
        {
            sceneGroupData = CreateInstance<SceneGroupData>();
            AssetDatabase.CreateAsset(sceneGroupData, assetPath);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnGUI()
    {
        float totalHeight = position.height - 50; // Adjust based on other elements in your window
        sceneAreaHeight = Mathf.Clamp(totalHeight * 0.5f, 100, totalHeight - 100); // Allocate half the height to scenes, but keep minimum and maximum limits

        mainScrollPosition = GUILayout.BeginScrollView(mainScrollPosition);

        GUILayout.Label("Scenes in Project", EditorStyles.boldLabel);

        if (GUILayout.Button("Create New Scene"))
        {
            CreateNewSceneWindow.ShowWindow(this);
        }

        DrawSearchBar();

        DrawScenesList(); // Handle drawing the scenes list with correct scroll behavior

        HandleDragAndDrop(); // Manage drag and drop operations

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
        if (draggingScene != null)
        {
            sceneScrollPosition = initialSceneScrollPosition; // Restore the initial scroll position during dragging
        }

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
            if (scene == selectedScene)
            {
                GUILayout.Space(5);
                if (GUILayout.Button("Rename", GUILayout.Width(60)))
                {
                    StartRenamingScene(scene);
                }
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
        Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(Path.GetFileNameWithoutExtension(scene)), sceneStyle);

        if (scene == selectedScene)
        {
            EditorGUI.DrawRect(labelRect, new Color(0.24f, 0.49f, 0.91f)); // Highlight color
            sceneStyle.normal.textColor = Color.white;
        }
        else
        {
            sceneStyle.normal.textColor = GUI.skin.label.normal.textColor;
        }

        if (Event.current.type == EventType.Repaint)
        {
            sceneStyle.Draw(labelRect, Path.GetFileNameWithoutExtension(scene), false, false, scene == selectedScene, false);
        }

        HandleSceneSelection(scene, labelRect);
    }

    private void HandleSceneSelection(string scene, Rect labelRect)
    {
        if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition))
        {
            initialMousePosition = Event.current.mousePosition;
            if (scene == selectedScene && (EditorApplication.timeSinceStartup - lastClickTime) < doubleClickThreshold)
            {
                OpenScene(scene);
            }
            else
            {
                selectedScene = scene;
                lastClickTime = (float)EditorApplication.timeSinceStartup; // Store the time of the click
                GUI.FocusControl(null); // Ensure the GUI control focus is updated properly
                Repaint(); // Repaint the window to reflect the new selection
            }
            Event.current.Use();
        }

        if (Event.current.type == EventType.MouseDrag)
        {
            if (draggingScene == null && selectedScene == scene)
            {
                if (Vector2.Distance(Event.current.mousePosition, initialMousePosition) > dragThreshold)
                {
                    draggingScene = selectedScene;
                    initialSceneScrollPosition = sceneScrollPosition; // Store the initial scroll position at the start of the drag
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new UnityEngine.Object[0];
                    DragAndDrop.StartDrag("DraggingScene");
                    Event.current.Use();
                }
            }
        }
    }

    private void StartRenamingScene(string scene)
    {
        renameScene = scene;
        newSceneName = Path.GetFileNameWithoutExtension(scene);
    }

    private void HandleDragAndDrop()
    {
        Event currentEvent = Event.current;

        if (currentEvent.type == EventType.DragUpdated && draggingScene != null)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            currentEvent.Use();
        }

        if (currentEvent.type == EventType.DragPerform && draggingScene != null)
        {
            DragAndDrop.AcceptDrag();
            if (sceneGroupData.sceneGroups.Count == 0)
            {
                EditorUtility.DisplayDialog("No Groups", "You must have at least one group to add the scene to.", "OK");
                draggingScene = null;
                currentEvent.Use();
                return;
            }
            if (targetGroup != null)
            {
                var group = sceneGroupData.sceneGroups.Find(g => g.groupName == targetGroup);
                if (group != null)
                {
                    if (group.scenes.Contains(draggingScene))
                    {
                        EditorUtility.DisplayDialog("Scene Already in Group", "The scene is already in the selected group.", "OK");
                    }
                    else
                    {
                        AddSceneToGroup(draggingScene, targetGroup);
                    }
                }
                draggingScene = null;
                targetGroup = null;
            }
            currentEvent.Use();

            Repaint();
        }

        if (currentEvent.type == EventType.MouseUp)
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
                if (sceneGroupData.sceneGroups.Count == 0)
                {
                    EditorUtility.DisplayDialog("No Groups", "You must have at least one group to add the scene to.", "OK");
                    return;
                }

                GenericMenu menu = new GenericMenu();
                foreach (var group in sceneGroupData.sceneGroups)
                {
                    string groupName = group.groupName;
                    menu.AddItem(new GUIContent(groupName), false, () => AddSceneToGroup(selectedScene, groupName));
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

        foreach (var group in sceneGroupData.sceneGroups)
        {
            if (!groupCollapsedStates.ContainsKey(group.groupName))
            {
                groupCollapsedStates[group.groupName] = false; // Initialize state if not present
            }

            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(groupCollapsedStates[group.groupName] ? "►" : "▼", GUILayout.Width(20)))
            {
                groupCollapsedStates[group.groupName] = !groupCollapsedStates[group.groupName];
            }
            GUILayout.Label(group.groupName, EditorStyles.boldLabel);
            GUILayout.EndHorizontal();

            if (!groupCollapsedStates[group.groupName])
            {
                Rect groupRect = GUILayoutUtility.GetLastRect();
                if (draggingScene != null && groupRect.Contains(Event.current.mousePosition))
                {
                    targetGroup = group.groupName;
                    GUI.Box(groupRect, GUIContent.none, new GUIStyle { normal = { background = EditorGUIUtility.whiteTexture }, border = new RectOffset(2, 2, 2, 2) });
                    Repaint();
                }

                GUILayout.BeginVertical();
                for (int i = group.scenes.Count - 1; i >= 0; i--)
                {
                    var scene = group.scenes[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("  - " + Path.GetFileNameWithoutExtension(scene), EditorStyles.label);

                    if (GUILayout.Button("Open", EditorStyles.miniButton, GUILayout.Width(60)))
                    {
                        OpenScene(scene);
                    }

                    if (GUILayout.Button("Remove", EditorStyles.miniButton, GUILayout.Width(70)))
                    {
                        group.scenes.RemoveAt(i);
                        i--;
                        Repaint();
                        break;
                    }

                    if (GUILayout.Button("Move", EditorStyles.miniButton, GUILayout.Width(70)))
                    {
                        GenericMenu menu = new GenericMenu();
                        foreach (var targetGroup in sceneGroupData.sceneGroups)
                        {
                            if (targetGroup.groupName != group.groupName)
                            {
                                string targetGroupName = targetGroup.groupName;
                                menu.AddItem(new GUIContent(targetGroupName), false, () => MoveSceneToGroup(scene, targetGroupName));
                            }
                        }
                        menu.ShowAsContext();
                    }

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove Group", EditorStyles.miniButton))
            {
                if (EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete the group '{group.groupName}'?", "Yes", "No"))
                {
                    sceneGroupData.sceneGroups.Remove(group);
                    Repaint();
                    break;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical(); // End the "box" vertical group for each group
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

        if (newGroupName.Length > maxGroupNameLength)
        {
            EditorUtility.DisplayDialog("Group Name Too Long", "Group name cannot be longer than 50 characters.", "OK");
            return;
        }

        if (!sceneGroupData.sceneGroups.Exists(group => group.groupName == newGroupName))
        {
            SceneGroupData.SceneGroup newGroup = new SceneGroupData.SceneGroup { groupName = newGroupName };
            sceneGroupData.sceneGroups.Add(newGroup);
            newGroupName = "";
            EditorUtility.SetDirty(sceneGroupData);
        }
    }

    private void AddSceneToGroup(string scene, string groupName)
    {
        var group = sceneGroupData.sceneGroups.Find(g => g.groupName == groupName);
        if (group != null && !group.scenes.Contains(scene))
        {
            group.scenes.Add(scene);
            EditorUtility.SetDirty(sceneGroupData);
        }
    }

    private void MoveSceneToGroup(string scene, string targetGroupName)
    {
        var targetGroup = sceneGroupData.sceneGroups.Find(g => g.groupName == targetGroupName);
        if (targetGroup != null)
        {
            foreach (var group in sceneGroupData.sceneGroups)
            {
                if (group.scenes.Contains(scene))
                {
                    group.scenes.Remove(scene);
                    break;
                }
            }
            if (!targetGroup.scenes.Contains(scene))
            {
                targetGroup.scenes.Add(scene);
                EditorUtility.SetDirty(sceneGroupData);
            }
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
