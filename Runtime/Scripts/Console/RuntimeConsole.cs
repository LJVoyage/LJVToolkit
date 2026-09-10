using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VoyageForge.Depot.Runtime.Utilities;

namespace VoyageForge.Depot.Runtime.Console
{
    /// <summary>
    /// 基于 UI Toolkit（UXML + USS）实现的运行时控制台。
    ///
    /// 职责：
    /// 1. 捕获 <see cref="Application.logMessageReceived"/> 产生的 Debug.Log / LogWarning / LogError / Exception；
    /// 2. 把日志渲染到可拖动、可折叠、可过滤的悬浮面板中；
    /// 3. 提供一个简单的命令输入框，支持注册自定义命令。
    ///
    /// 使用方式：
    /// - 继承自 MonoSingleton，可被继承；
    /// - 推荐在业务侧的 [RuntimeInitializeOnLoadMethod] 中调用 <see cref="Initialize"/> 完成初始化（默认隐藏）；
    /// - 运行后连按 3 次 Tab 键唤醒 / 切换显隐。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]   // 依赖 UIDocument 承载 UI Toolkit 面板
    [DisallowMultipleComponent]              // 同一 GameObject 上只允许存在一个
    public class RuntimeConsole : MonoSingleton<RuntimeConsole>
    {
        // 资源路径：UXML / USS / PanelSettings 都放在 Resources/Depot/Console/ 目录下，
        // 通过 Resources.Load 按“无扩展名”的路径加载，依靠具体类型区分是哪个资源。
        private const string UxmlResourcePath = "Depot/Console/RuntimeConsole";
        private const string UssResourcePath = "Depot/Console/RuntimeConsole";
        private const string PanelSettingsResourcePath = "Depot/Console/RuntimeConsole";

        // ---------------------------------------------------------------
        // 可配置字段（Inspector 中可见）
        // ---------------------------------------------------------------

        [Header("Log")]
        [SerializeField, Min(1)] private int _maxEntries = 300;   // 日志缓冲上限，超出后丢弃最旧的一条

        [Header("Panel")]
        [SerializeField] private bool _visibleOnInitialize;        // 初始化后是否立即显示（默认隐藏）
        [SerializeField] private bool _draggable = true;           // 是否允许拖动标题栏移动面板

        [Header("Wake")]
        [SerializeField] private KeyCode _wakeKey = KeyCode.Tab;   // 唤醒按键
        [SerializeField, Min(2)] private int _wakeTapCount = 3;    // 需要连按的次数
        [SerializeField, Min(0.1f)] private float _wakeTapWindow = 0.5f; // 两次按键之间的最大间隔（秒），超过则重新计数

        [Header("Input")]
        [SerializeField] private bool _enableCommandInput = true;  // 是否启用底部命令输入框

        // ---------------------------------------------------------------
        // UI 元素引用
        // ---------------------------------------------------------------

        private UIDocument _document;           // 承载整个面板的 UIDocument 组件
        private VisualElement _panel;           // UXML 克隆出来的模板容器（TemplateContainer），同时作为 flex 居中容器
        private VisualElement _consoleRoot;     // 面板根元素（console-root），控制显隐与拖拽的对象
        private VisualElement _header;          // 标题栏，用于拖拽
        private ScrollView _list;               // 日志列表
        private TextField _commandInput;        // 命令输入框

        // ---------------------------------------------------------------
        // 日志与过滤状态
        // ---------------------------------------------------------------

        private readonly List<ConsoleLogEntry> _entries = new List<ConsoleLogEntry>(); // 日志缓冲
        private readonly Dictionary<ConsoleFilter, VisualElement> _filterButtons = new Dictionary<ConsoleFilter, VisualElement>(); // 过滤标签元素
        private readonly Dictionary<ConsoleFilter, Label> _filterCounts = new Dictionary<ConsoleFilter, Label>(); // 过滤标签上的数量角标
        private ConsoleFilter _activeFilter = ConsoleFilter.All;  // 当前激活的过滤类型

        // 各级别日志计数，用于显示过滤标签上的数量角标
        private int _countLog;
        private int _countWarning;
        private int _countError;

        // ---------------------------------------------------------------
        // 拖拽状态
        // ---------------------------------------------------------------

        private bool _dragging;                 // 是否正在拖拽
        private Vector2 _dragStartPointer;      // 拖拽开始时的指针位置
        private Vector2 _dragStartTranslate;    // 拖拽开始时的累积位移
        private Vector2 _dragTranslate;         // 当前累积位移（通过 translate 应用到面板上）

        // ---------------------------------------------------------------
        // 唤醒状态
        // ---------------------------------------------------------------

        private int _wakeTaps;                                      // 已累计的唤醒按键次数
        private float _lastWakeTapTime = float.NegativeInfinity;    // 上一次按键时间（初始为负无穷，确保第一次一定重置）

        // ---------------------------------------------------------------
        // 命令注册表（静态，所有实例共享）
        // ---------------------------------------------------------------

        private static readonly Dictionary<string, Action<string[]>> Commands =
            new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>控制台当前是否可见（静态，无实例时返回 false）。</summary>
        public static bool IsVisible => HasInstance && Instance.IsShown;

        /// <summary>当前实例是否可见。</summary>
        public bool IsShown => _consoleRoot != null && _consoleRoot.style.display == DisplayStyle.Flex;

        /// <summary>新日志写入缓冲后触发，供外部监听日志流。</summary>
        public static event Action<ConsoleLogEntry> EntryLogged;

        // ---------------------------------------------------------------
        // 初始化
        // ---------------------------------------------------------------

        /// <summary>
        /// 创建/获取控制台单例。
        /// 首次调用会创建 GameObject 并完成面板构建，但不会显示面板（默认隐藏）。
        /// 推荐在业务侧的 [RuntimeInitializeOnLoadMethod] 方法中调用。
        /// </summary>
        /// <returns>控制台单例实例。</returns>
        public static RuntimeConsole Initialize()
        {
            // 访问 MonoSingleton 的 Instance 会触发惰性创建，完成 OnInitialize -> BuildPanel 全流程
            return Instance;
        }

        // ---------------------------------------------------------------
        // 生命周期（继承自 MonoSingleton）
        // ---------------------------------------------------------------

        /// <summary>
        /// MonoSingleton 初始化回调（在 Awake 完成实例注册后自动调用）。
        /// 负责构建面板、注册内置命令，然后派发 <see cref="OnConsoleInitialized"/>。
        /// 派生类若重写，务必先调用 base.OnInitialize()。
        /// </summary>
        protected override void OnInitialize()
        {
            BuildPanel();             // 加载 UXML/USS/PanelSettings 并装配 UI
            RegisterBuiltInCommands(); // 注册 clear / help / log 内置命令
            OnConsoleInitialized();    // 通知派生类：初始化完成
        }

        /// <summary>面板构建完成、内置命令注册后调用。派生类可重写以追加初始化逻辑。</summary>
        protected virtual void OnConsoleInitialized() { }

        /// <summary>控制台显隐状态发生变化时调用。派生类可重写。</summary>
        /// <param name="visible">变化后是否可见。</param>
        protected virtual void OnVisibilityChanged(bool visible) { }

        /// <summary>每收到一条日志时调用（在写入缓冲之后）。派生类可重写。</summary>
        /// <param name="entry">刚写入的日志条目。</param>
        protected virtual void OnLogReceived(ConsoleLogEntry entry) { }

        /// <summary>组件启用时订阅 Unity 日志回调。</summary>
        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        /// <summary>组件禁用时取消订阅，避免重复/悬挂回调。</summary>
        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        /// <summary>每帧检测唤醒按键。</summary>
        private void Update()
        {
            HandleWakeInput();
        }

        // ---------------------------------------------------------------
        // 唤醒
        // ---------------------------------------------------------------

        /// <summary>
        /// 检测“连按 N 次唤醒键”的输入。
        /// 两次按键间隔超过 <see cref="_wakeTapWindow"/> 就重新计数；
        /// 累计达到 <see cref="_wakeTapCount"/> 次时切换面板显隐。
        /// 使用 Time.unscaledTime，保证 timeScale=0（暂停）时也能唤醒。
        /// </summary>
        private void HandleWakeInput()
        {
            // 仅当唤醒键在“本帧刚按下”时才处理
            if (!Input.GetKeyDown(_wakeKey))
            {
                return;
            }

            float now = Time.unscaledTime;

            // 距离上一次按键太久，重置计数
            if (now - _lastWakeTapTime > _wakeTapWindow)
            {
                _wakeTaps = 0;
            }

            _lastWakeTapTime = now;
            _wakeTaps++;

            // 连按次数达标，切换显隐并清空计数
            if (_wakeTaps >= _wakeTapCount)
            {
                _wakeTaps = 0;
                _lastWakeTapTime = float.NegativeInfinity;
                Toggle();
            }
        }

        // ---------------------------------------------------------------
        // 面板构建
        // ---------------------------------------------------------------

        /// <summary>
        /// 构建整个面板：准备 UIDocument 与 PanelSettings，加载并装配 UXML/USS，
        /// 查询关键元素，绑定过滤、按钮、命令输入与拖拽事件。
        /// </summary>
        private void BuildPanel()
        {
            _document = GetComponent<UIDocument>();

            // 优先使用 Resources 中的 PanelSettings 资源；缺失则回退到运行时创建一份
            if (_document.panelSettings == null)
            {
                _document.panelSettings = Resources.Load<PanelSettings>(PanelSettingsResourcePath);
                if (_document.panelSettings == null)
                {
                    Debug.LogWarning($"[RuntimeConsole] 未找到 PanelSettings 资源：{PanelSettingsResourcePath}，回退到运行时创建的默认设置。");
                    _document.panelSettings = CreatePanelSettings();
                }
            }

            // 设置较大的排序值，让控制台渲染在所有 UI 之上
            _document.sortingOrder = 32767;

            // 加载 UXML（必须存在，否则无法构建面板）
            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>(UxmlResourcePath);
            if (uxml == null)
            {
                Debug.LogError($"[RuntimeConsole] 未找到 UXML 资源：{UxmlResourcePath}。请确认文件位于 Resources 目录下。");
                return;
            }

            // 加载 USS（可选，缺失仅提示警告）
            StyleSheet uss = Resources.Load<StyleSheet>(UssResourcePath);
            if (uss == null)
            {
                Debug.LogWarning($"[RuntimeConsole] 未找到 USS 资源：{UssResourcePath}。");
            }

            // 克隆 UXML，得到模板容器（TemplateContainer）作为 _panel
            _panel = uxml.CloneTree();

            // 让 _panel 填满根容器，并用 flex 布局把 console-root 居中（水平 + 垂直）
            _panel.style.flexGrow = 1;
            _panel.style.width = new Length(100, LengthUnit.Percent);
            _panel.style.height = new Length(100, LengthUnit.Percent);
            _panel.style.alignItems = Align.Center;
            _panel.style.justifyContent = Justify.Center;

            _document.rootVisualElement.Add(_panel);

            // 把 USS 直接挂到面板容器（_panel）上，确保样式对 console-root 及其所有子元素生效。
            // 挂到 rootVisualElement 在部分情况下可能被 PanelSettings 的主题样式表覆盖。
            if (uss != null)
            {
                _panel.styleSheets.Add(uss);
                Debug.Log($"[RuntimeConsole] 已应用 USS 样式：{UssResourcePath}");
            }

            // 查询 UXML 中的关键元素
            _consoleRoot = _panel.Q<VisualElement>("console-root");
            _header = _panel.Q<VisualElement>("console-header");
            _list = _panel.Q<ScrollView>("console-list");
            _commandInput = _panel.Q<TextField>("console-command-input");

            // 绑定过滤标签
            BindFilterButton(_panel, "filter-all", ConsoleFilter.All);
            BindFilterButton(_panel, "filter-log", ConsoleFilter.Log);
            BindFilterButton(_panel, "filter-warning", ConsoleFilter.Warning);
            BindFilterButton(_panel, "filter-error", ConsoleFilter.Error);

            // 绑定标题栏按钮（仅保留关闭按钮）
            _panel.Q<Button>("console-close-button")?.RegisterCallback<ClickEvent>(_ => Hide());

            // 绑定命令输入框：启用时监听回车，禁用时直接隐藏
            if (_commandInput != null)
            {
                if (_enableCommandInput)
                {
                    // 禁用聚焦/点击输入框时的“全选”行为，避免点击日志后再输入时把已有内容覆盖掉
                    _commandInput.selectAllOnFocus = false;
                    _commandInput.selectAllOnMouseUp = false;
                    _commandInput.RegisterCallback<KeyDownEvent>(OnCommandKeyDown);
                }
                else
                {
                    _commandInput.style.display = DisplayStyle.None;
                }
            }

            // 绑定拖拽事件
            if (_draggable && _header != null)
            {
                _header.RegisterCallback<PointerDownEvent>(OnHeaderPointerDown);
                _header.RegisterCallback<PointerMoveEvent>(OnHeaderPointerMove);
                _header.RegisterCallback<PointerUpEvent>(OnHeaderPointerUp);
                _header.RegisterCallback<PointerCaptureOutEvent>(_ => _dragging = false);
            }

            // 初始化界面状态：过滤高亮、数量角标、显隐、列表
            ApplyFilterVisual();
            UpdateFilterCounts();
            SetVisible(_visibleOnInitialize);
            RebuildList();
        }

        /// <summary>
        /// 运行时创建一份默认的 PanelSettings 作为回退方案。
        /// 使用 ScaleWithScreenSize 缩放，参考分辨率 1920x1080。
        /// </summary>
        private static PanelSettings CreatePanelSettings()
        {
            PanelSettings settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.name = "Depot RuntimeConsole PanelSettings";
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.match = 0.5f;
            return settings;
        }

        // ---------------------------------------------------------------
        // 日志处理
        // ---------------------------------------------------------------

        /// <summary>
        /// Unity 日志回调：把收到的日志封装成条目写入缓冲，更新计数与界面。
        /// </summary>
        /// <param name="condition">日志正文。</param>
        /// <param name="stackTrace">调用堆栈。</param>
        /// <param name="type">日志类型。</param>
        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            // 记录当前时间戳，格式 HH:mm:ss.fff
            ConsoleLogEntry entry = new ConsoleLogEntry(
                condition,
                stackTrace,
                type,
                DateTime.Now.ToString("HH:mm:ss.fff"));

            _entries.Add(entry);

            // 超出缓冲上限时丢弃最旧的一条，并同步扣减对应计数
            if (_entries.Count > _maxEntries)
            {
                DecrementCount(_entries[0].Type);
                _entries.RemoveAt(0);
            }

            // 累加新日志对应级别的计数
            IncrementCount(type);

            // 通知外部监听者与派生类
            EntryLogged?.Invoke(entry);
            OnLogReceived(entry);

            // 刷新过滤标签上的数量角标
            UpdateFilterCounts();

            // 只有面板可见时才重建列表，避免无谓开销
            if (IsShown)
            {
                RebuildList();
            }
        }

        /// <summary>按当前过滤条件重建日志列表。</summary>
        private void RebuildList()
        {
            if (_list == null)
            {
                return;
            }

            _list.Clear();

            // 按缓冲顺序（旧 -> 新）渲染，符合控制台习惯，之后滚动到底部
            foreach (ConsoleLogEntry entry in _entries)
            {
                if (!MatchesFilter(entry.Type))
                {
                    continue;
                }

                _list.Add(BuildEntryRow(entry));
            }

            ScrollToBottom();
        }

        /// <summary>根据日志条目构建一行 UI：箭头 + 时间戳 + 消息，有堆栈时附可展开的堆栈块。</summary>
        /// <param name="entry">日志条目。</param>
        /// <returns>构建好的行元素。</returns>
        private VisualElement BuildEntryRow(ConsoleLogEntry entry)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("console-entry");
            row.AddToClassList(GetEntryClass(entry.Type)); // 根据级别附加 log / warning / error 样式类

            bool hasStack = !string.IsNullOrEmpty(entry.StackTrace);

            // 行内结构：级别圆点 | 箭头 | 时间戳 | 消息
            VisualElement line = new VisualElement();
            line.AddToClassList("console-entry-line");

            // 级别指示圆点（颜色由 USS 按级别着色）
            VisualElement dot = new VisualElement();
            dot.AddToClassList("console-entry-dot");
            line.Add(dot);

            // 有堆栈时显示展开箭头，无堆栈则留空占位
            Label caret = new Label(hasStack ? "▸" : string.Empty);
            caret.AddToClassList("console-entry-caret");
            line.Add(caret);

            Label timestamp = new Label(entry.Timestamp);
            timestamp.AddToClassList("console-entry-ts");
            line.Add(timestamp);

            Label message = new Label(entry.Message);
            message.AddToClassList("console-entry-msg");
            line.Add(message);

            row.Add(line);

            if (hasStack)
            {
                // 堆栈块默认隐藏，点击行切换显隐并旋转箭头
                Label trace = new Label(entry.StackTrace);
                trace.AddToClassList("console-entry-stack");
                trace.style.display = DisplayStyle.None;
                row.Add(trace);

                line.RegisterCallback<ClickEvent>(evt =>
                {
                    // 阻止点击冒泡，避免影响命令输入框的焦点/选中状态
                    evt.StopPropagation();

                    // 记录切换前是否在列表底部，用于展开/收起后恢复视图位置
                    bool wasAtBottom = IsAtBottom();

                    bool expanded = trace.style.display != DisplayStyle.None;
                    trace.style.display = expanded ? DisplayStyle.None : DisplayStyle.Flex;
                    caret.text = expanded ? "▸" : "▾";

                    // 若之前在底部，展开/收起后都保持在底部，避免内容高度变化导致视图错位
                    if (wasAtBottom)
                    {
                        ScrollToBottom();
                    }
                });
            }

            return row;
        }

        /// <summary>判断某条日志是否匹配当前激活的过滤类型。</summary>
        /// <param name="type">日志类型。</param>
        /// <returns>是否匹配。</returns>
        private bool MatchesFilter(LogType type)
        {
            switch (_activeFilter)
            {
                case ConsoleFilter.Log:
                    return type == LogType.Log;
                case ConsoleFilter.Warning:
                    return type == LogType.Warning;
                case ConsoleFilter.Error:
                    // Error 过滤包含 Error / Assert / Exception 三类
                    return type == LogType.Error || type == LogType.Assert || type == LogType.Exception;
                default:
                    return true;
            }
        }

        /// <summary>根据日志类型返回对应的 USS 样式类名。</summary>
        /// <param name="type">日志类型。</param>
        /// <returns>样式类名。</returns>
        private static string GetEntryClass(LogType type)
        {
            switch (type)
            {
                case LogType.Warning:
                    return "console-entry-warning";
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    return "console-entry-error";
                default:
                    return "console-entry-log";
            }
        }

        /// <summary>把日志列表滚动到底部（延迟到布局完成后执行）。</summary>
        private void ScrollToBottom()
        {
            if (_list == null)
            {
                return;
            }

            // 连续两帧尝试滚动到底部，覆盖“面板刚由隐藏变为显示、布局尚未更新”时
            // verticalScroller.highValue 还是旧值（0）的情况，确保最终定位到最新日志。
            _list.schedule.Execute(() =>
            {
                ScrollToBottomNow();
                _list.schedule.Execute(ScrollToBottomNow);
            });
        }

        /// <summary>立即把日志列表滚动到底部（供延迟调度调用）。</summary>
        private void ScrollToBottomNow()
        {
            Scroller scroller = _list.verticalScroller;
            if (scroller != null)
            {
                scroller.value = scroller.highValue;
            }
        }

        /// <summary>判断日志列表当前是否滚动到底部（允许少量误差）。</summary>
        private bool IsAtBottom()
        {
            if (_list == null)
            {
                return false;
            }

            Scroller scroller = _list.verticalScroller;
            if (scroller == null)
            {
                return true;
            }

            return scroller.value >= scroller.highValue - 2f;
        }

        // ---------------------------------------------------------------
        // 可见性与折叠
        // ---------------------------------------------------------------

        /// <summary>显示控制台。</summary>
        public void Show()
        {
            SetVisible(true);
        }

        /// <summary>隐藏控制台。</summary>
        public void Hide()
        {
            SetVisible(false);
        }

        /// <summary>切换控制台显隐。</summary>
        public void Toggle()
        {
            SetVisible(!IsShown);
        }

        /// <summary>设置控制台显隐状态。</summary>
        /// <param name="visible">是否可见。</param>
        public void SetVisible(bool visible)
        {
            if (_consoleRoot == null)
            {
                return;
            }

            bool wasShown = IsShown;
            _consoleRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

            // 显示时刷新列表，保证日志是最新的
            if (visible)
            {
                RebuildList();
            }

            // 状态确实发生变化时通知派生类
            if (wasShown != visible)
            {
                OnVisibilityChanged(visible);
            }
        }

        /// <summary>静态：显示当前单例。</summary>
        public static void ShowInstance()
        {
            Instance?.Show();
        }

        /// <summary>静态：隐藏当前单例。</summary>
        public static void HideInstance()
        {
            Instance?.Hide();
        }

        /// <summary>静态：切换当前单例显隐。</summary>
        public static void ToggleInstance()
        {
            Instance?.Toggle();
        }

        /// <summary>清空日志缓冲与计数，并刷新界面。</summary>
        public void Clear()
        {
            _entries.Clear();
            _countLog = 0;
            _countWarning = 0;
            _countError = 0;
            UpdateFilterCounts();
            RebuildList();
        }

        // ---------------------------------------------------------------
        // 过滤
        // ---------------------------------------------------------------

        /// <summary>绑定一个过滤标签：缓存元素与数量角标，并注册点击事件。</summary>
        /// <param name="tree">搜索根节点。</param>
        /// <param name="name">过滤标签在 UXML 中的名称。</param>
        /// <param name="filter">对应的过滤类型。</param>
        private void BindFilterButton(VisualElement tree, string name, ConsoleFilter filter)
        {
            VisualElement button = tree.Q<VisualElement>(name);
            if (button == null)
            {
                return;
            }

            _filterButtons[filter] = button;

            // 找到标签内的数量角标 Label（按 class 查找）
            Label count = button.Q<Label>(className: "console-filter-count");
            if (count != null)
            {
                _filterCounts[filter] = count;
            }

            // 点击后切换激活过滤类型并刷新列表
            button.RegisterCallback<ClickEvent>(_ =>
            {
                _activeFilter = filter;
                ApplyFilterVisual();
                RebuildList();
            });
        }

        /// <summary>刷新过滤标签的“激活”高亮样式。</summary>
        private void ApplyFilterVisual()
        {
            foreach (KeyValuePair<ConsoleFilter, VisualElement> pair in _filterButtons)
            {
                pair.Value.EnableInClassList("console-filter-button-active", pair.Key == _activeFilter);
            }
        }

        /// <summary>根据日志类型累加对应级别的计数。</summary>
        /// <param name="type">日志类型。</param>
        private void IncrementCount(LogType type)
        {
            if (type == LogType.Log)
            {
                _countLog++;
            }
            else if (type == LogType.Warning)
            {
                _countWarning++;
            }
            else if (IsErrorType(type))
            {
                _countError++;
            }
        }

        /// <summary>根据日志类型扣减对应级别的计数（用于缓冲溢出丢弃旧日志时）。</summary>
        /// <param name="type">日志类型。</param>
        private void DecrementCount(LogType type)
        {
            if (type == LogType.Log)
            {
                _countLog--;
            }
            else if (type == LogType.Warning)
            {
                _countWarning--;
            }
            else if (IsErrorType(type))
            {
                _countError--;
            }
        }

        /// <summary>判断是否为“错误类”日志（Error / Assert / Exception 都归入 Error）。</summary>
        /// <param name="type">日志类型。</param>
        /// <returns>是否属于错误类。</returns>
        private static bool IsErrorType(LogType type)
        {
            return type == LogType.Error || type == LogType.Assert || type == LogType.Exception;
        }

        /// <summary>把各级别计数写回过滤标签上的数量角标。</summary>
        private void UpdateFilterCounts()
        {
            if (_filterCounts.TryGetValue(ConsoleFilter.All, out Label all))
            {
                all.text = (_countLog + _countWarning + _countError).ToString();
            }

            if (_filterCounts.TryGetValue(ConsoleFilter.Log, out Label log))
            {
                log.text = _countLog.ToString();
            }

            if (_filterCounts.TryGetValue(ConsoleFilter.Warning, out Label warning))
            {
                warning.text = _countWarning.ToString();
            }

            if (_filterCounts.TryGetValue(ConsoleFilter.Error, out Label error))
            {
                error.text = _countError.ToString();
            }
        }

        // ---------------------------------------------------------------
        // 拖拽
        // ---------------------------------------------------------------

        /// <summary>
        /// 标题栏按下：若不是点击按钮，则开始拖拽。
        /// 记录起始指针位置与起始位移，并捕获指针。
        /// </summary>
        private void OnHeaderPointerDown(PointerDownEvent evt)
        {
            // 只响应鼠标左键
            if (evt.button != 0 || _consoleRoot == null)
            {
                return;
            }

            // 点击标题栏上的按钮（折叠/清空/关闭）时不触发拖拽
            if (IsInsideButton(evt.target as VisualElement))
            {
                return;
            }

            _dragging = true;
            _dragStartPointer = evt.position;
            _dragStartTranslate = _dragTranslate;
            _header.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        /// <summary>判断一个元素是否位于某个 Button 内部（沿父链向上查找）。</summary>
        /// <param name="element">起始元素。</param>
        /// <returns>是否在 Button 内。</returns>
        private static bool IsInsideButton(VisualElement element)
        {
            while (element != null)
            {
                if (element is Button)
                {
                    return true;
                }

                element = element.parent;
            }

            return false;
        }

        /// <summary>拖拽移动：根据指针位移更新 translate，实现面板跟随。</summary>
        private void OnHeaderPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging || _consoleRoot == null || !_header.HasPointerCapture(evt.pointerId))
            {
                return;
            }

            // 用“本次指针位移 + 起始位移”得到新的累积位移
            Vector2 delta = (Vector2)evt.position - _dragStartPointer;
            _dragTranslate = _dragStartTranslate + delta;

            // translate 不影响 flex 布局位置，因此拖拽不会破坏居中布局
            _consoleRoot.style.translate = new Translate(_dragTranslate.x, _dragTranslate.y);
        }

        /// <summary>拖拽结束：释放指针捕获并结束拖拽状态。</summary>
        private void OnHeaderPointerUp(PointerUpEvent evt)
        {
            if (_dragging && _header != null && _header.HasPointerCapture(evt.pointerId))
            {
                _header.ReleasePointer(evt.pointerId);
            }

            _dragging = false;
        }

        // ---------------------------------------------------------------
        // 命令
        // ---------------------------------------------------------------

        /// <summary>注册一个自定义命令（命令输入框回车后执行）。</summary>
        /// <param name="name">命令名（不区分大小写）。</param>
        /// <param name="handler">命令处理函数，参数为空格分隔的参数数组。</param>
        public static void RegisterCommand(string name, Action<string[]> handler)
        {
            if (string.IsNullOrWhiteSpace(name) || handler == null)
            {
                return;
            }

            Commands[name.Trim()] = handler;
        }

        /// <summary>注销一个已注册的命令。</summary>
        /// <param name="name">命令名。</param>
        /// <returns>是否成功移除。</returns>
        public static bool UnregisterCommand(string name)
        {
            return Commands.Remove(name);
        }

        /// <summary>注册内置命令：clear / help / log。</summary>
        private static void RegisterBuiltInCommands()
        {
            // 清空日志
            RegisterCommand("clear", _ => Instance?.Clear());

            // 列出所有可用命令
            RegisterCommand("help", _ =>
            {
                Debug.Log($"[Console] 可用命令：{string.Join(", ", Commands.Keys)}");
            });

            // 打印一段文本（把参数拼回成一句话）
            RegisterCommand("log", args =>
            {
                Debug.Log($"[Console] {string.Join(" ", args)}");
            });
        }

        /// <summary>命令输入框按键处理：回车时解析并执行命令。</summary>
        private void OnCommandKeyDown(KeyDownEvent evt)
        {
            // 仅响应主回车与小键盘回车
            if (_commandInput == null ||
                (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter))
            {
                return;
            }

            // 取出输入内容并清空输入框
            string raw = _commandInput.value;
            _commandInput.value = string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            // 按空格拆分：第一段为命令名，其余为参数
            string[] parts = raw.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return;
            }

            string command = parts[0];
            string[] args = new string[parts.Length - 1];
            Array.Copy(parts, 1, args, 0, args.Length);

            // 查找并执行命令；异常与未知命令都给出提示
            if (Commands.TryGetValue(command, out Action<string[]> handler))
            {
                try
                {
                    handler(args);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Console] 命令 '{command}' 执行失败：{ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[Console] 未知命令：{command}");
            }
        }
    }
}
