using System.IO;
using UnityEditor;
using UnityEngine;

namespace LJVToolkit.Editor.Scripts.Utilities
{
    [FilePath("ProjectSettings/LJVToolkitSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class LJVToolkitProjectSettings : ScriptableSingleton<LJVToolkitProjectSettings>
    {
        private const string SettingsAssetPath = "ProjectSettings/LJVToolkitSettings.asset";

        [SerializeField] private bool autoVersionEnabled = true;
        [SerializeField] private int autoVersionIncrementStep = 1;
        [SerializeField] private bool skipSplashEnabled;

        public bool AutoVersionEnabled
        {
            get => autoVersionEnabled;
            set
            {
                if (autoVersionEnabled == value)
                {
                    return;
                }

                autoVersionEnabled = value;
                Save(true);
            }
        }

        public bool SkipSplashEnabled
        {
            get => skipSplashEnabled;
            set
            {
                if (skipSplashEnabled == value)
                {
                    return;
                }

                skipSplashEnabled = value;
                Save(true);
            }
        }

        public int AutoVersionIncrementStep
        {
            get => Mathf.Max(1, autoVersionIncrementStep);
            set
            {
                int sanitizedValue = Mathf.Max(1, value);
                if (autoVersionIncrementStep == sanitizedValue)
                {
                    return;
                }

                autoVersionIncrementStep = sanitizedValue;
                Save(true);
            }
        }

        public void EnsureSaved()
        {
            bool changed = false;
            if (autoVersionIncrementStep < 1)
            {
                autoVersionIncrementStep = 1;
                changed = true;
            }

            if (changed || !File.Exists(SettingsAssetPath))
            {
                Save(true);
            }
        }
    }
}
