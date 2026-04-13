using UnityEditor;

namespace LJVToolkit.Editor.Scripts.Utilities
{
    public static class SkipSplashEditor
    {
        private const string MENU_ITEM_PATH = "LJV/Toolkit/Skip Splash";
        private const string SKIP_SPLASH_DEFINE = "SKIP_SPLASH";

        /// <summary>
        /// 跳过启动画面
        /// </summary>
        [MenuItem(MENU_ITEM_PATH)]
        public static void SkipSplash()
        {
            var settings = LJVToolkitProjectSettings.instance;
            settings.EnsureSaved();
            settings.SkipSplashEnabled = !settings.SkipSplashEnabled;
            ApplySetting(settings.SkipSplashEnabled);
        }

        [InitializeOnLoadMethod]
        private static void InitializeMenu()
        {
            EditorApplication.delayCall += () =>
            {
                var settings = LJVToolkitProjectSettings.instance;
                settings.EnsureSaved();
                ApplySetting(settings.SkipSplashEnabled);
            };
        }

        public static void ApplySetting(bool skipSplashEnabled)
        {
            if (skipSplashEnabled)
            {
                AddSkipSplashDefine();
            }
            else
            {
                RemoveSkipSplashDefine();
            }

            UpdateMenuValidation(skipSplashEnabled);
        }

        private static void AddSkipSplashDefine()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            if (!currentDefines.Contains(SKIP_SPLASH_DEFINE))
            {
                currentDefines += $";{SKIP_SPLASH_DEFINE}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, currentDefines);
            }
        }

        private static void RemoveSkipSplashDefine()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            if (currentDefines.Contains(SKIP_SPLASH_DEFINE))
            {
                currentDefines = currentDefines.Replace(SKIP_SPLASH_DEFINE, "").Replace(";;", ";").Trim(';');
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, currentDefines);
            }
        }


        private static void UpdateMenuValidation(bool? isEnabled = null)
        {
            bool checkedState = isEnabled ?? HasSkipSplashDefine();
            Menu.SetChecked(MENU_ITEM_PATH, checkedState);
        }

        private static bool HasSkipSplashDefine()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            return currentDefines.Contains(SKIP_SPLASH_DEFINE);
        }
    }
}
