# SceneOrganizer

SceneOrganizer is a Unity Editor tool designed to simplify the management of scenes within your Unity project. This tool allows you to create, organize, and manage scenes and scene groups effortlessly. It provides a user-friendly interface for handling scene assets, making it easier to navigate and work with complex projects.

## Features

### Scene Management
- **Scene List:** Display all scenes within your project, sorted alphabetically.
- **Search Bar:** Quickly find scenes by typing keywords in the search bar.
- **Scene Operations:**
  - **Open Scene:** Open a selected scene in the editor.
  - **Rename Scene:** Rename any scene directly within the SceneOrganizer window.
  - **Create New Scene:** Create new scenes using predefined templates.

### Group Management
- **Scene Groups:** Organize your scenes into groups for better project management.
- **Add New Group:** Create new groups to categorize your scenes.
- **Add Scene to Group:** Drag and drop scenes into groups or use the context menu to add selected scenes to a group.
- **Move Scenes Between Groups:** Move scenes between different groups easily.
- **Remove Scene from Group:** Remove scenes from groups without deleting the scene.
- **Remove Group:** Delete a group and optionally remove all its scenes from the project.

### Drag and Drop Functionality
- **Drag Scenes:** Drag scenes from the scene list and drop them into groups.
- **Visual Feedback:** Highlight groups when a scene is dragged over them.

### Customization
- **Resizable Panels:** Adjust the height of the scene list panel to suit your preferences.
- **Group Collapsing:** Collapse and expand groups to focus on specific parts of your project.

### Utility
- **Watermark:** Displays a customizable watermark in the editor window.

## Installation

1. Clone or download the repository.
2. Place the `SceneOrganizer` folder in your project's `Assets/Editor` directory.

## Usage

1. Open the SceneOrganizer window from the Unity menu: `Window > Scene Organizer`.
2. Use the interface to manage your scenes and scene groups.

## Documentation

### SceneOrganizerWindow

#### Properties
- `List<string> scenes`: List of scenes in the project.
- `SceneGroupData sceneGroupData`: Data structure for storing scene groups.
- `string newGroupName`: Name for creating new groups.
- `Vector2 mainScrollPosition`, `sceneScrollPosition`, `groupScrollPosition`, `initialSceneScrollPosition`: Scroll positions for various scrollable areas.
- `float sceneAreaHeight`: Height of the scene list area.
- `bool isResizing`: Indicates if the user is resizing the scene list area.
- `string draggingScene`: Scene currently being dragged.
- `string targetGroup`: Target group for the dragged scene.
- `string searchQuery`: Search query for filtering scenes.
- `string selectedScene`: Currently selected scene.
- `string renameScene`: Scene currently being renamed.
- `string newSceneName`: New name for the scene being renamed.
- `float lastClickTime`: Timestamp of the last click event.
- `bool isGroupScrollViewActive`: Indicates if the group scroll view is active.
- `Dictionary<string, bool> groupCollapsedStates`: States for collapsing and expanding groups.

#### Methods
- `ShowWindow()`: Opens the SceneOrganizer window.
- `OnEnable()`: Loads scenes and groups when the window is enabled.
- `OnDisable()`: Saves groups when the window is disabled.
- `LoadScenes()`: Loads all scenes in the project.
- `SaveGroups()`: Saves the current state of scene groups.
- `LoadGroups()`: Loads scene groups from the asset.
- `OnGUI()`: Renders the GUI for the SceneOrganizer window.
- `DrawSearchBar()`: Draws the search bar for filtering scenes.
- `DrawScenesList()`: Draws the list of scenes.
- `DrawSceneItem(string scene)`: Draws an individual scene item.
- `DrawRenameSceneField(string scene)`: Draws the rename field for a scene.
- `DrawSceneLabel(string scene, GUIStyle sceneStyle)`: Draws the label for a scene.
- `HandleSceneSelection(string scene, Rect labelRect)`: Handles the selection of scenes.
- `StartRenamingScene(string scene)`: Initiates the renaming of a scene.
- `HandleDragAndDrop()`: Manages drag and drop operations.
- `DrawResizeHandle()`: Draws the handle for resizing the scene list area.
- `DrawAddSceneToGroupButton()`: Draws the button for adding a scene to a group.
- `DrawNewGroupSection()`: Draws the section for creating new groups.
- `DrawGroupSection()`: Draws the section for managing groups.
- `RenameScene(string oldScenePath, string newSceneName)`: Renames a scene.
- `AddNewGroup()`: Adds a new group.
- `AddSceneToGroup(string scene, string groupName)`: Adds a scene to a group.
- `MoveSceneToGroup(string scene, string targetGroupName)`: Moves a scene to a different group.
- `OpenScene(string scenePath)`: Opens a scene in the editor.
- `DrawWatermark()`: Draws the watermark in the editor window.

### SceneGroupData

#### Properties
- `List<SceneGroup> sceneGroups`: List of scene groups.

#### Nested Classes
- `SceneGroup`: Represents a group of scenes.
  - `string groupName`: Name of the group.
  - `List<string> scenes`: List of scenes in the group.

### CreateNewSceneWindow

#### Properties
- `string sceneName`: Name of the new scene.
- `string[] sceneTemplates`: Array of scene templates.
- `int selectedTemplateIndex`: Index of the selected scene template.
- `string savePath`: Path where the new scene will be saved.
- `SceneOrganizerWindow organizerWindow`: Reference to the SceneOrganizer window.

#### Methods
- `ShowWindow(SceneOrganizerWindow organizerWindow)`: Opens the CreateNewSceneWindow.
- `OnGUI()`: Renders the GUI for the CreateNewSceneWindow.
- `CreateScene()`: Creates a new scene based on the specified template.

## Contributing

Feel free to submit issues or pull requests. Contributions are welcome!

## License

This project is licensed under the MIT License.

## Credits

Made by [Nativvstudios](https://nativvstudios.com).

---
