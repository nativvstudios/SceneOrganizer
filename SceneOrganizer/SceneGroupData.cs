using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneGroupData", menuName = "ScriptableObjects/SceneGroupData", order = 1)]
public class SceneGroupData : ScriptableObject
{
    public List<SceneGroup> sceneGroups = new List<SceneGroup>();

    [System.Serializable]
    public class SceneGroup
    {
        public string groupName;
        public List<string> scenes = new List<string>();
    }
}
