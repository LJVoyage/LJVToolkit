using System;
using LJVoyage.LJVToolkit.Runtime.Attributes;
using UnityEngine;

namespace LJVoyage.LJVToolkit.Runtime.Utilities
{
    [Serializable]
    public class SceneReference 
    {
#if UNITY_EDITOR
        public const string GuidFieldName = nameof(_sceneGuid);
        public const string NameFieldName = nameof(_sceneName);
        public const string PathFieldName = nameof(_scenePath);
#endif

        [SerializeField, HideInInspector] private string _sceneGuid;
        [SerializeField, HideInInspector] private string _sceneName;
        [SerializeField, HideInInspector] private string _scenePath;

        public string SceneGuid
        {
            get => _sceneGuid;
            private set => _sceneGuid = value;
        }

        public string SceneName
        {
            get => _sceneName;
            private set => _sceneName = value;
        }

        public string ScenePath
        {
            get => _scenePath;
            private set => _scenePath = value;
        }
    }
}