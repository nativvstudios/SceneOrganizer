using UnityEditor;
using UnityEngine;
using System;
using System.IO;

public class SettingsWindow : EditorWindow
{
    private static SceneOrganizerWindow organizerWindow;
    private bool enableBackup;
    private string backupDirectory;
    private string[] backupFiles;
    private int selectedBackupIndex;

    public static void ShowWindow(SceneOrganizerWindow window)
    {
        organizerWindow = window;
        SettingsWindow windowInstance = GetWindow<SettingsWindow>("SceneOrganizer Settings", true);
        windowInstance.LoadBackupFiles();
        windowInstance.AdjustWindowSize();
    }

    private void OnEnable()
    {
        // Load saved settings
        enableBackup = EditorPrefs.GetBool("SceneOrganizer_EnableBackup", false);
        backupDirectory = EditorPrefs.GetString("SceneOrganizer_BackupDirectory", Application.dataPath);
        
        if (string.IsNullOrEmpty(backupDirectory))
        {
            backupDirectory = Application.dataPath;
            EditorPrefs.SetString("SceneOrganizer_BackupDirectory", backupDirectory);
        }
        
        LoadBackupFiles();
        AdjustWindowSize();
    }

    private void LoadBackupFiles()
    {
        try
        {
            if (!string.IsNullOrEmpty(backupDirectory) && Directory.Exists(backupDirectory))
            {
                backupFiles = Directory.GetFiles(backupDirectory, "SceneGroupData_Backup_*.asset");
                Array.Sort(backupFiles);
                for (int i = 0; i < backupFiles.Length; i++)
                {
                    backupFiles[i] = Path.GetFileName(backupFiles[i]);
                }
            }
            else
            {
                backupFiles = new string[0];
            }
            selectedBackupIndex = backupFiles.Length > 0 ? backupFiles.Length - 1 : -1;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading backup files: {ex.Message}");
            backupFiles = new string[0];
            selectedBackupIndex = -1;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("SceneOrganizer Settings", EditorStyles.boldLabel);

        // Backup settings
        enableBackup = EditorGUILayout.Toggle("Enable Backup", enableBackup);
        if (enableBackup)
        {
            EditorGUILayout.BeginHorizontal();
            backupDirectory = EditorGUILayout.TextField("Backup Directory", backupDirectory);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedDirectory = EditorUtility.OpenFolderPanel("Select Backup Directory", backupDirectory, "");
                if (!string.IsNullOrEmpty(selectedDirectory))
                {
                    backupDirectory = selectedDirectory;
                    LoadBackupFiles();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        GUILayout.Label("Restore from Backup", EditorStyles.boldLabel);

        if (backupFiles != null && backupFiles.Length > 0 && selectedBackupIndex >= 0 && selectedBackupIndex < backupFiles.Length)
        {
            selectedBackupIndex = EditorGUILayout.Popup("Select Backup", selectedBackupIndex, backupFiles);

            if (GUILayout.Button("Restore from Selected Backup"))
            {
                RestoreFromBackup(backupFiles[selectedBackupIndex]);
            }
        }
        else
        {
            GUILayout.Label("No backups available", EditorStyles.miniLabel);
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save"))
        {
            SaveSettings();
            Close();
        }

        AdjustWindowSize();
    }

    private void SaveSettings()
    {
        // Make sure backupDirectory is not null
        if (string.IsNullOrEmpty(backupDirectory))
        {
            backupDirectory = Application.dataPath;
        }

        // Save settings
        EditorPrefs.SetBool("SceneOrganizer_EnableBackup", enableBackup);
        EditorPrefs.SetString("SceneOrganizer_BackupDirectory", backupDirectory);

        // Notify the main window of the settings change
        if (organizerWindow != null)
        {
            organizerWindow.UpdateBackupSettings(enableBackup, backupDirectory);
        }
    }

    private void RestoreFromBackup(string backupFileName)
    {
        try
        {
            string backupPath = Path.Combine(backupDirectory, backupFileName);
            string assetFullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", SceneOrganizerWindow.assetPath));
            
            // Ensure the directory exists
            string directory = Path.GetDirectoryName(assetFullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(backupPath, assetFullPath, true);
            AssetDatabase.Refresh();
            
            if (organizerWindow != null)
            {
                organizerWindow.LoadGroups();
            }
            
            Debug.Log("Restored SceneGroupData from selected backup.");
            EditorUtility.DisplayDialog("Restore from Backup", "SceneGroupData successfully restored from the selected backup.", "OK");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to restore SceneGroupData from backup: {ex.Message}");
            EditorUtility.DisplayDialog("Restore from Backup", "Failed to restore SceneGroupData from backup: " + ex.Message, "OK");
        }
    }

    private void AdjustWindowSize()
    {
        // Adjust the window size to fit the content
        float height = 120f; // Base height
        if (enableBackup)
        {
            height += 40f; // Add height for backup directory field
        }
        height += backupFiles.Length > 0 ? 60f : 20f; // Add height for backup selection

        height += 40f; // Add height for save button and spacing

        minSize = new Vector2(300, height);
        maxSize = new Vector2(300, height);
    }
}