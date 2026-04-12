using System;
using LJVoyage.LJVToolkit.Runtime.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace LJVToolkit.Editor.Scripts.Inspector
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceEditor : PropertyDrawer
    {
        private static VisualTreeAsset _visualTreeAsset;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (_visualTreeAsset == null)
            {
                _visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/LJVToolkit/Editor/Scripts/Inspector/SceneReference/SceneReferenceInspector.uxml");
            }

            var rootElement = _visualTreeAsset.CloneTree();

            SerializedProperty guidProperty = property.FindPropertyRelative(SceneReference.GuidFieldName);
            SerializedProperty pathProperty = property.FindPropertyRelative(SceneReference.PathFieldName);
            SerializedProperty nameProperty = property.FindPropertyRelative(SceneReference.NameFieldName);

            if (guidProperty == null || pathProperty == null || nameProperty == null)
            {
                return new Label("SceneReference fields not found");
            }

            var objectField = rootElement.Q<ObjectField>("SceneAssetField");
            var noSceneHint = rootElement.Q<VisualElement>("NoSceneHint");
            var sceneDetails = rootElement.Q<VisualElement>("SceneDetails");
            var pathLabel = sceneDetails.Q<VisualElement>("ScenePath").Q<Label>("Value");
            var nameLabel = sceneDetails.Q<VisualElement>("SceneName").Q<Label>("Value");
            var detailsTitle = sceneDetails.Q<Label>();

            objectField.objectType = typeof(SceneAsset);

            // 初始化
            UpdateDisplay(guidProperty, objectField, noSceneHint, sceneDetails, pathLabel, nameLabel, detailsTitle);

            // 值改变事件
            objectField.RegisterValueChangedCallback(evt =>
            {
                property.serializedObject.Update();

                if (evt.newValue == null)
                {
                    guidProperty.stringValue = string.Empty;
                    pathProperty.stringValue = string.Empty;
                    nameProperty.stringValue = string.Empty;
                }
                else if (evt.newValue is SceneAsset sceneAsset)
                {
                    string path = AssetDatabase.GetAssetPath(sceneAsset);
                    string guid = AssetDatabase.AssetPathToGUID(path);

                    guidProperty.stringValue = guid;
                    pathProperty.stringValue = path;
                    nameProperty.stringValue = sceneAsset.name;
                }

                UpdateDisplay(guidProperty, objectField, noSceneHint, sceneDetails, pathLabel, nameLabel, detailsTitle);
                property.serializedObject.ApplyModifiedProperties();
            });

            return rootElement;
        }

        private void UpdateDisplay(
            SerializedProperty guidProperty,
            ObjectField objectField,
            VisualElement noSceneHint,
            VisualElement sceneDetails,
            Label pathLabel,
            Label nameLabel,
            Label detailsTitle)
        {
            string guid = guidProperty.stringValue;
            bool hasScene = !string.IsNullOrEmpty(guid);

            if (!hasScene)
            {
                noSceneHint.style.display = DisplayStyle.Flex;
                sceneDetails.style.display = DisplayStyle.None;
                objectField.value = null;
                return;
            }

            noSceneHint.style.display = DisplayStyle.None;
            sceneDetails.style.display = DisplayStyle.Flex;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            if (string.IsNullOrEmpty(path))
            {
                nameLabel.text = "无效的场景GUID";
                pathLabel.text = "请重新选择";
                sceneDetails.style.backgroundColor = new StyleColor(new Color(0.95f, 0.8f, 0.8f));
                detailsTitle.style.color = new StyleColor(Color.red);
                objectField.value = null;
                return;
            }

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            
            if (sceneAsset == null)
            {
                nameLabel.text = "场景资源丢失";
                pathLabel.text = path;
                sceneDetails.style.backgroundColor = new StyleColor(new Color(1f, 0.95f, 0.8f));
               
                detailsTitle.style.color = new StyleColor(new Color(1f, 0.6f, 0f));
                objectField.value = null;
                return;
            }

            objectField.value = sceneAsset;
            nameLabel.text = sceneAsset.name;
            pathLabel.text = path;
            
            // 重置为正常颜色
            sceneDetails.style.backgroundColor = new StyleColor(new Color(0.91f, 0.96f, 0.91f)); // #E8F5E9
           
            detailsTitle.style.color = new StyleColor(new Color(0.18f, 0.49f, 0.20f));           // #2E7D32
        }
    }
}