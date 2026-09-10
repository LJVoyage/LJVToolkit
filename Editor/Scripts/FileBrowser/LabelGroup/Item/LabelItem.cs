using System;
using UnityEngine;
using UnityEngine.Events;
using VoyageForge.Depot.Editor.Utilities;
using UnityEngine.UIElements;

namespace VoyageForge.Depot.Editor.ProjectBrowser
{
    public sealed class LabelItem : VFVisualElement
    {
        /// <summary>
        /// 激活时样式
        /// </summary>
        private const string _activeClassName = "label-item-active";

        /// <summary>
        /// 聚焦时样式
        /// </summary>
        private const string _hoverClassName = "label-item-hover";

        private readonly Label _label;

        private readonly Button _button;

        private readonly VisualElement _templateContainer;

        /// <summary>
        /// 关闭事件
        /// </summary>
        public event Action<LabelItem> OnClosed;

        /// <summary>
        /// 激活 事件
        /// </summary>
        public event Action<LabelItem> OnActived;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                ToggleActive();
            }
        }

        private bool _isActive = false;

        /// <summary>
        /// 标签项键值
        /// </summary>
        public string Key => _key;

        private string _key;

        public LabelItem(string label)
        {
            _templateContainer = TreeAsset.InstantiateWithFillAndAddTo(this);


            _templateContainer.AddToClassList("label-item");

            _label = this.Q<Label>("label");

            _key = label;
            _label.text = label;

            _button = this.Q<Button>("button");

            RegisterCallback<ClickEvent>(OnClick);
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

            _button.RegisterCallback<ClickEvent>(OnCloseClick);
        }

        private void OnCloseClick(ClickEvent evt)
        {
            evt.StopPropagation();

            OnClosed?.Invoke(this);
        }

        /// <summary>
        /// 鼠标离开元素时调用
        /// </summary>
        /// <param name="evt"></param>
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            _templateContainer.RemoveFromClassList(_hoverClassName);
        }

        /// <summary>
        /// 鼠标进入时调用
        /// </summary>
        /// <param name="evt"></param>
        private void OnMouseEnter(MouseEnterEvent evt)
        {
            if (!_isActive)
                _templateContainer.AddToClassList(_hoverClassName);
        }


        /// <summary>
        /// 切换激活状态
        /// </summary>
        public void ToggleActive()
        {
            if (IsActive)
            {
                OnActived?.Invoke(this);
                _templateContainer.AddToClassList(_activeClassName);
                _templateContainer.RemoveFromClassList(_hoverClassName);
            }
            else
            {
                _templateContainer.RemoveFromClassList(_activeClassName);
            }
        }

        /// <summary>
        /// 点击激活
        /// </summary>
        /// <param name="evt"></param>
        private void OnClick(ClickEvent evt)
        {
            evt.StopPropagation(); // 是否需要停止传播？原本没有，但如果有需要可保留
            if (!IsActive)
            {
                ToggleActive();
            }
        }
    }
}