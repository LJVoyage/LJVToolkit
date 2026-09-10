using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace VoyageForge.Depot.Editor.Utilities
{
    /// <summary>
    /// 搜索范围枚举
    /// </summary>
    public enum SearchScope
    {
        /// <summary>仅限当前脚本所属的包内；若脚本不在包中，则回退到 Assets 文件夹</summary>
        PackageOnly,

        /// <summary>整个项目（所有 Assets 和 Packages）</summary>
        Global
    }

    public static class UxmlUtility
    {
        /// <summary>
        /// 根据指定的搜索范围查找 UXML 文件。
        /// </summary>
        /// <param name="fileNameWithoutExtension">文件名（不含 .uxml）</param>
        /// <param name="scope">搜索范围：PackageOnly 或 Global</param>
        /// <returns>相对路径（可直接用于 AssetDatabase.LoadAssetAtPath），未找到返回 null</returns>
        public static string FindUxmlPath(string fileNameWithoutExtension, SearchScope scope = SearchScope.PackageOnly)
        {
            string[] searchFolders = null;

            var packageInfo = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());

            if (scope == SearchScope.PackageOnly)
            {
                if (packageInfo != null)
                {
                    // 当前脚本在 Package 中 → 限定在该包根目录
                    searchFolders = new[] { $"Packages/{packageInfo.name}" };
                }
                else
                {
                    // 当前脚本不在任何 Package 中（例如放在 Assets 下）→ 回退到 Assets 文件夹
                    searchFolders = new[] { "Assets" };
                    Debug.LogWarning("当前脚本不在 Package 中，将在 Assets 文件夹下搜索 UXML。");
                }
            }
            // else Global → searchFolders 保持 null，即搜索整个项目

            // 执行搜索：类型为 VisualTreeAsset，名称匹配
            string[] guids = AssetDatabase.FindAssets(
                $"t:VisualTreeAsset {fileNameWithoutExtension}",
                searchFolders
            );

            if (guids.Length == 0)
            {
                string scopeDesc = scope == SearchScope.PackageOnly
                    ? (packageInfo != null ? "当前包内" : "Assets 文件夹下")
                    : "全局";
                Debug.LogWarning($"在{scopeDesc}未找到名为 {fileNameWithoutExtension} 的 UXML 文件。");
                return null;
            }

            if (guids.Length > 1)
            {
                string scopeDesc = scope == SearchScope.PackageOnly
                    ? (packageInfo != null ? "当前包内" : "Assets 文件夹下")
                    : "全局";
                Debug.LogWarning($"在{scopeDesc}找到多个名为 {fileNameWithoutExtension} 的 UXML 文件，将使用第一个。");
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return path.EndsWith(".uxml") ? path : null;
        }


        /// <summary>
        ///  在包内加载 UXML 文件（默认行为）。
        /// </summary>
        /// <param name="fileNameWithoutExtension"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static VisualTreeAsset LoadVisualTreeAsset(string fileNameWithoutExtension,
            SearchScope scope = SearchScope.PackageOnly)
        {
            var assetPath = FindUxmlPath(fileNameWithoutExtension, scope);

            if (!string.IsNullOrEmpty(assetPath))
            {
                var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                if (visualTreeAsset != null)
                {
                    return visualTreeAsset;
                }
            }

            throw new FileNotFoundException($"无法找到 UXML 资源：{fileNameWithoutExtension}");
        }
    }
}