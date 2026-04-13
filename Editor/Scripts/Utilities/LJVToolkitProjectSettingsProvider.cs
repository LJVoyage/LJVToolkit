using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace LJVToolkit.Editor.Scripts.Utilities
{
    public static class LJVToolkitProjectSettingsProvider
    {
        private const string SettingsUxmlPath = "Assets/LJVToolkit/Editor/Scripts/Utilities/LJVToolkitProjectSettings.uxml";
        private static VisualTreeAsset _settingsVisualTreeAsset;

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Project/LJV Toolkit", SettingsScope.Project)
            {
                label = "LJV Toolkit",
                activateHandler = (_, rootElement) =>
                {
                    var settings = LJVToolkitProjectSettings.instance;
                    settings.EnsureSaved();

                    rootElement.Clear();
                    var visualTreeAsset = LoadSettingsVisualTreeAsset();
                    if (visualTreeAsset == null)
                    {
                        rootElement.Add(new Label("LJV Toolkit Project Settings UXML not found."));
                        return;
                    }

                    visualTreeAsset.CloneTree(rootElement);

                    var autoVersionToggle = rootElement.Q<Toggle>("AutoVersionToggle");
                    var incrementField = rootElement.Q<IntegerField>("AutoVersionIncrementField");
                    var skipSplashToggle = rootElement.Q<Toggle>("SkipSplashToggle");

                    autoVersionToggle.value = settings.AutoVersionEnabled;
                    autoVersionToggle.RegisterValueChangedCallback(evt =>
                    {
                        settings.AutoVersionEnabled = evt.newValue;
                    });

                    incrementField.value = settings.AutoVersionIncrementStep;
                    incrementField.RegisterValueChangedCallback(evt =>
                    {
                        int sanitizedValue = evt.newValue < 1 ? 1 : evt.newValue;
                        if (incrementField.value != sanitizedValue)
                        {
                            incrementField.SetValueWithoutNotify(sanitizedValue);
                        }

                        settings.AutoVersionIncrementStep = sanitizedValue;
                    });

                    skipSplashToggle.value = settings.SkipSplashEnabled;
                    skipSplashToggle.RegisterValueChangedCallback(evt =>
                    {
                        settings.SkipSplashEnabled = evt.newValue;
                        SkipSplashEditor.ApplySetting(evt.newValue);
                    });
                },
                keywords = new HashSet<string>(new[] { "LJV", "Toolkit", "Auto", "Version", "Build", "Skip", "Splash" })
            };
        }

        private static VisualTreeAsset LoadSettingsVisualTreeAsset()
        {
            if (_settingsVisualTreeAsset != null)
            {
                return _settingsVisualTreeAsset;
            }

            _settingsVisualTreeAsset = UxmlAssetUtility.LoadVisualTreeAsset(
                SettingsUxmlPath);
            return _settingsVisualTreeAsset;
        }
    }
}
