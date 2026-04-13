using System.IO;
using UnityEditor;
using UnityEngine.UIElements;

namespace VoyageForge.Depot.Editor.Scripts.Utilities
{
    /// <summary>
    /// 编辑器 UXML 资源加载工具。
    /// 优先按固定路径加载，路径失效时再按资源名搜索，
    /// 这样既能保证常规情况下的加载性能，也能在资源移动后保留一定的容错能力。
    /// </summary>
    public static class UxmlAssetUtility
    {
        /// <summary>
        /// 加载指定路径的 UXML 资源。
        ///
        /// 使用示例：
        /// var tree = UxmlAssetUtility.LoadVisualTreeAsset(
        ///     "Assets/Depot/Editor/Scripts/Utilities/DepotProjectSettings.uxml");
        /// tree.CloneTree(rootElement);
        /// </summary>
        /// <param name="defaultAssetPath">资源的默认路径。</param>
        /// <returns>找到的 VisualTreeAsset 资源。</returns>
        /// <exception cref="FileNotFoundException">当资源无法通过路径或名称找到时抛出。</exception>
        public static VisualTreeAsset LoadVisualTreeAsset(string defaultAssetPath)
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(defaultAssetPath);
            if (visualTreeAsset != null)
            {
                return visualTreeAsset;
            }

            string assetName = Path.GetFileNameWithoutExtension(defaultAssetPath);
            string[] guids = AssetDatabase.FindAssets($"{assetName} t:VisualTreeAsset");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
                if (visualTreeAsset != null)
                {
                    return visualTreeAsset;
                }
            }

            throw new FileNotFoundException($"无法找到 UXML 资源：{defaultAssetPath}");
        }
    }
}
