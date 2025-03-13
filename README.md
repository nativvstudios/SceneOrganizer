# Scene Organizer

![Unity Version](https://img.shields.io/badge/Unity-2019.4%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)

A Unity Editor tool that helps you organize, manage, and quickly access scenes in your project. Group scenes together for better workflow management and faster development.

![Scene Organizer Preview](https://via.placeholder.com/800x450.png?text=Scene+Organizer+Screenshot)

## Features

- **Scene Organization**: Manage scenes by creating custom groups
- **Quick Scene Access**: Open scenes directly from the organizer window
- **Drag-and-Drop Support**: Easily add scenes to groups with drag-and-drop
- **Scene Group Management**: Create, rename, and delete scene groups
- **Scene Search**: Quickly find scenes with the search bar
- **Scene Renaming**: Rename scenes directly from the organizer window
- **Scene Moving**: Move scenes between groups
- **Backup System**: Automatic backup of your scene organization data
- **Restore From Backup**: Recover your scene organization from previous backups

## Installation

### Method 1: Unity Package

1. Download the latest `.unitypackage` release from the [Releases](https://github.com/yourusername/scene-organizer/releases) page
2. Open your Unity project
3. Import the package via `Assets > Import Package > Custom Package...`
4. Select the downloaded `.unitypackage` file

### Method 2: Manual Installation

1. Clone this repository or download the ZIP file
2. Copy the `Assets/Scripts/Utilities/SceneOrganizer` folder into your project's Assets folder

## Getting Started

1. Open the Scene Organizer window via `Window > Scene Organizer`
2. Create a new group by entering a name and clicking "Add Group"
3. Select a scene from the list and add it to your group
4. Double-click on any scene to open it in the editor

## Usage Guide

### Creating Scene Groups

1. Enter a group name in the "New Group Name" field
2. Click "Add Group" or press Enter
3. Groups will be displayed in the lower section of the window

### Adding Scenes to Groups

**Method 1: Using the Button**
1. Select a scene from the upper list
2. Click "Add Selected Scene to Group"
3. Choose the target group from the dropdown menu

**Method 2: Using Drag and Drop**
1. Click and hold on a scene in the upper list
2. Drag the scene onto the desired group
3. Release to add the scene to that group

### Managing Scenes in Groups

- **Open Scene**: Click the "Open" button next to a scene in a group
- **Remove Scene**: Click the "Remove" button to remove a scene from a group (this doesn't delete the scene file)
- **Move Scene**: Click the "Move" button to move a scene to a different group

### Group Management

- **Collapse/Expand Groups**: Click the ▼/► button next to each group name
- **Rename Group**: Click the "Rename" button next to a group name
- **Remove Group**: Click the "Remove Group" button at the bottom of each group section

### Creating New Scenes

1. Click the "Create New Scene" button
2. Enter a name for your new scene
3. Choose a template (Default or Empty)
4. Select a save location
5. Click "Create Scene"

### Backup & Restore

1. Click the settings (⚙️) icon in the search bar to access backup settings
2. Enable backups and set a backup directory
3. To restore from a backup, select a backup file from the dropdown and click "Restore from Selected Backup"

## Configuration

### Backup Settings

- **Enable Backup**: Toggle automatic backups of your scene organization data
- **Backup Directory**: Set the directory where backups will be stored
- The system keeps the 3 most recent backups

## File Structure

```
Assets/
└── Scripts/
    └── Utilities/
        └── SceneOrganizer/
            ├── SceneOrganizerWindow.cs      # Main organizer window
            ├── CreateNewSceneWindow.cs      # Scene creation window
            ├── SettingsWindow.cs            # Settings and backup window
            └── SceneGroupData.cs            # Data structure for scene groups
```

## Data Storage

Scene organization data is stored in a ScriptableObject located at `Assets/Editor/SceneGroupData.asset`. This file is created automatically the first time you use the tool.

## Compatibility

- Unity 2019.4 or higher
- Works with both the Built-in Render Pipeline and URP/HDRP projects

## Contributing

Contributions are welcome! Feel free to submit a pull request or create an issue if you have suggestions or encounter bugs.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgements

- Made by [Nativvstudios](https://github.com/nativvstudios)
- Thanks to all contributors who have helped improve this tool