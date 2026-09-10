using System.Collections.Generic;
using System.Linq;
using VoyageForge.Depot.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VoyageForge.Depot.Editor.ProjectBrowser
{
    public sealed class LabelGroup : VFVisualElement
    {
        private const string _fileLabelItemClassName = "file-label-item";

        private VisualElement _container;


        // ---------- UxmlFactory 和 UxmlTraits（支持 UI Builder 和 UXML 序列化） ----------
        public new class UxmlFactory : UxmlFactory<LabelGroup, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }


        public LabelGroup()
        {
            _container = TreeAsset.InstantiateWithFillAndAddTo(this);

            RegisterCallback<PointerDownEvent>(OnClick);
        }

        /// <summary>
        /// 标签项字典
        /// </summary>
        private readonly Dictionary<string, LabelItem> _items = new();

        /// <summary>
        /// 标签项索引器
        /// </summary>
        /// <param name="key"></param>
        public LabelItem this[string key] => _items[key];
        
        /// <summary>
        /// 标签项数量
        /// </summary>
        public int Count => _items.Count;
        
        /// <summary>
        /// 当前激活的标签项
        /// </summary>
        private LabelItem _currentLabel;

        /// <summary>
        /// 添加标签项
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public LabelItem AddLabel(string label)
        {
            var item = new LabelItem(label);

            _container.Add(item);

            _items.Add(label, item);

            return item;
        }

        private void OnClick(PointerDownEvent evt)
        {
            if (evt.button == 1)
            {
                CreateLabel(GUID.Generate().ToString());
                evt.StopImmediatePropagation();
            }
        }

        /// <summary>
        /// 创建标签项
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public LabelItem CreateLabel(string label)
        {
            var item = new LabelItem(label);

            _items.Add(label, item);

            _container.Add(item);

            item.OnClosed += OnClosed;
            item.OnActived += OnActivated;

            if (_items.Count == 1)
            {
                _currentLabel = item;
                item.IsActive = true;
            }

            return item;
        }

        /// <summary>
        /// 标签项激活事件
        /// </summary>
        /// <param name="label"></param>
        private void OnActivated(LabelItem label)
        {
            foreach (var item in _items.Values)
            {
                if (item != label)
                {
                    item.IsActive = false;
                }
            }

            label.IsActive = true;
        }

        /// <summary>
        /// 标签项关闭事件
        /// </summary>
        /// <param name="label"></param>
        private void OnClosed(LabelItem label)
        {
            if (_items.Count == 1)
            {
                return;
            }

            _items.Remove(label.Key);

            if (label.IsActive)
            {
                label.IsActive = false;
                _currentLabel = _items.Values.First();
                _currentLabel.IsActive = true;
            }


            _container.Remove(label);
        }
    }
}