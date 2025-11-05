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
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            // 添加或移除定义
            if (currentDefines.Contains(SKIP_SPLASH_DEFINE))
            {
                RemoveSkipSplashDefine();
            }
            else
            {
                AddSkipSplashDefine();
            }

            // 更新菜单状态
            UpdateMenuValidation();
        }

        [InitializeOnLoadMethod]
        private static void InitializeMenu()
        {
            // 在编辑器加载时初始化菜单状态
            EditorApplication.delayCall += UpdateMenuValidation;
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


        private static void UpdateMenuValidation()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            Menu.SetChecked(MENU_ITEM_PATH, currentDefines.Contains(SKIP_SPLASH_DEFINE));
        }
    }
}