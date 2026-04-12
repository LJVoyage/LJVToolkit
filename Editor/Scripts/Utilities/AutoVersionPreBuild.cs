using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LJVToolkit.Editor.Scripts.Utilities
{
    public class AutoVersionPreBuild: IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // 1. 获取当前 bundleVersion
            string version = PlayerSettings.bundleVersion; // e.g. "1.0.0"
            Debug.Log("Current Version: " + version);

            // 2. 拆分版本号
            string[] parts = version.Split('.');
            int major = int.Parse(parts[0]);
            int minor = int.Parse(parts[1]);
            int patch = int.Parse(parts[2]);

            // 3. 自动递增 patch
            patch++;
            string newVersion = $"{major}.{minor}.{patch}";

            // 更新 PlayerSettings 版本号
            PlayerSettings.bundleVersion = newVersion;

            // 4. Android versionCode 自增
            if (report.summary.platform == BuildTarget.Android)
            {
                PlayerSettings.Android.bundleVersionCode++;
                Debug.Log("Android VersionCode: " + PlayerSettings.Android.bundleVersionCode);
            }

            // 5. 打印新版本
            Debug.Log($"[AutoVersion] Updated Version: {newVersion}");
        }
    }
}