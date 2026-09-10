using VoyageForge.Depot.Editor.Utilities;
using UnityEditor;
using UnityEngine.UIElements;

namespace VoyageForge.Depot.Editor
{
    /// <summary>
    /// VF 元素 基类
    /// </summary>
    public abstract class VFVisualElement : VisualElement
    {
        /// <summary>
        /// 自动加载与类名相同的 uxml 文件
        /// </summary>
        protected virtual VisualTreeAsset TreeAsset
        {
            get { return UxmlUtility.LoadVisualTreeAsset(GetType().Name); }
        }
    }
}