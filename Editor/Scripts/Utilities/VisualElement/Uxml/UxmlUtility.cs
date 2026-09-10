using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        /// <param name="fileNameOrPath">
        /// 文件名或相对路径。既可以是纯文件名（如 "DepotProjectSettings"），
        /// 也可以是带 .uxml 扩展名或完整相对路径（如 "Assets/.../DepotProjectSettings.uxml"）；
        /// 内部会自动提取资源名用于名称匹配。
        /// </param>
        /// <param name="scope">搜索范围：PackageOnly 或 Global</param>
        /// <param name="ownerAssembly">
        /// 调用方所属程序集，用于定位调用方所在的包/Assets 范围；传 null 时自动取调用方程序集。
        /// </param>
        /// <returns>相对路径（可直接用于 AssetDatabase.LoadAssetAtPath），未找到返回 null</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string FindUxmlPath(string fileNameOrPath, SearchScope scope = SearchScope.PackageOnly, Assembly ownerAssembly = null)
        {
            // 跨程序集兼容：未显式传入时使用“调用方程序集”，
            // 这样 Bridge 等其它程序集调用本工具时，会按其所在的包/Assets 搜索，
            // 而不是按本工具类所在的 Depot 程序集搜索。
            ownerAssembly = ownerAssembly ?? Assembly.GetCallingAssembly();

            // 兼容模式：调用方可能误传相对路径或带扩展名的文件名，
            // 这里统一提取为纯资源名，供 AssetDatabase.FindAssets 按名称匹配。
            string fileNameWithoutExtension = NormalizeUxmlName(fileNameOrPath);

            string[] searchFolders = null;

            var packageInfo = PackageInfo.FindForAssembly(ownerAssembly);

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
        /// 兼容处理：把“相对路径 / 带扩展名文件名”统一转换为纯资源名（不含目录、不含扩展名）。
        /// 例如 "Assets/Depot/Editor/Scripts/Utilities/DepotProjectSettings.uxml" → "DepotProjectSettings"，
        /// "DepotProjectSettings.uxml" → "DepotProjectSettings"，
        /// "DepotProjectSettings" → "DepotProjectSettings"。
        /// AssetDatabase.FindAssets 的名称过滤只匹配资源名（不含路径和扩展名），
        /// 因此这里必须先去掉目录与扩展名，否则永远匹配不到。
        /// </summary>
        /// <param name="fileNameOrPath">文件名或相对路径。</param>
        /// <returns>可用于 AssetDatabase.FindAssets 名称过滤的资源名。</returns>
        private static string NormalizeUxmlName(string fileNameOrPath)
        {
            if (string.IsNullOrWhiteSpace(fileNameOrPath))
            {
                return fileNameOrPath;
            }

            // 判断是否为路径：包含目录分隔符（正斜杠或反斜杠）即视为路径。
            bool isPath = fileNameOrPath.Contains('/') || fileNameOrPath.Contains('\\');

            // Path.GetFileNameWithoutExtension 对“裸文件名 / 带扩展名文件名 / 相对路径”三种形式都能正确提取：
            // 取路径末段，并去掉最后一个扩展名。
            string assetName = Path.GetFileNameWithoutExtension(fileNameOrPath);

            // 仅在确认为路径时输出一次兼容提示，便于排查调用方是否误传路径。
            if (isPath)
            {
                Debug.Log($"[UxmlUtility] 传入的是相对路径（{fileNameOrPath}），已兼容提取文件名：{assetName}");
            }

            return assetName;
        }


        /// <summary>
        ///  在包内加载 UXML 文件（默认行为）。
        /// </summary>
        /// <param name="fileNameOrPath">文件名或相对路径（见 <see cref="FindUxmlPath"/>）。</param>
        /// <param name="scope"></param>
        /// <param name="ownerAssembly">调用方所属程序集；传 null 时自动取调用方程序集。</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static VisualTreeAsset LoadVisualTreeAsset(string fileNameOrPath,
            SearchScope scope = SearchScope.PackageOnly, Assembly ownerAssembly = null)
        {
            // 捕获“调用 LoadVisualTreeAsset 的调用方程序集”，
            // 并显式下传给 FindUxmlPath，避免在其内部再取到本类所在的 Depot 程序集。
            ownerAssembly = ownerAssembly ?? Assembly.GetCallingAssembly();

            var assetPath = FindUxmlPath(fileNameOrPath, scope, ownerAssembly);

            if (!string.IsNullOrEmpty(assetPath))
            {
                var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                if (visualTreeAsset != null)
                {
                    return visualTreeAsset;
                }
            }

            throw new FileNotFoundException($"无法找到 UXML 资源：{fileNameOrPath}");
        }
    }
}