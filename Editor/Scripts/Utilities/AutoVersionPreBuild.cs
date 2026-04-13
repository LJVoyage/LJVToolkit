using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LJVToolkit.Editor.Scripts.Utilities
{
    public class AutoVersionPreBuild : IPreprocessBuildWithReport
    {
        private const string ToggleMenuPath = "LJV/Toolkit/Auto Version";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var settings = LJVToolkitProjectSettings.instance;
            settings.EnsureSaved();

            if (!settings.AutoVersionEnabled)
            {
                Debug.Log("[AutoVersion] Auto increment disabled. Skipping version update.");
                return;
            }

            string version = PlayerSettings.bundleVersion;
            Debug.Log("Current Version: " + version);

            string[] parts = version.Split('.');
            int major = int.Parse(parts[0]);
            int minor = int.Parse(parts[1]);
            int patch = int.Parse(parts[2]);
            int incrementStep = settings.AutoVersionIncrementStep;

            patch += incrementStep;
            string newVersion = $"{major}.{minor}.{patch}";

            PlayerSettings.bundleVersion = newVersion;

            if (report.summary.platform == BuildTarget.Android)
            {
                PlayerSettings.Android.bundleVersionCode += incrementStep;
                Debug.Log("Android VersionCode: " + PlayerSettings.Android.bundleVersionCode);
            }

            Debug.Log($"[AutoVersion] Updated Version: {newVersion} (step: {incrementStep})");
        }

        [MenuItem(ToggleMenuPath)]
        private static void ToggleAutoIncrement()
        {
            var settings = LJVToolkitProjectSettings.instance;
            settings.EnsureSaved();
            settings.AutoVersionEnabled = !settings.AutoVersionEnabled;
            Debug.Log($"[AutoVersion] Auto increment {(settings.AutoVersionEnabled ? "enabled" : "disabled")}.");
        }

        [MenuItem(ToggleMenuPath, true)]
        private static bool ToggleAutoIncrementValidate()
        {
            var settings = LJVToolkitProjectSettings.instance;
            settings.EnsureSaved();
            Menu.SetChecked(ToggleMenuPath, settings.AutoVersionEnabled);
            return true;
        }
    }
}
