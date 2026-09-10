# Changelog

本文件由开发者手动维护，发布流程不会自动生成或修改此文件。

## Unreleased

### Added
- 新增基于 UI Toolkit（UXML + USS）的运行时控制台 `RuntimeConsole`：捕获 `Debug.Log/Warning/Error/Exception`，支持级别过滤与计数、堆栈展开、标题栏拖拽、命令输入（`help`/`clear`/`log`），连按 3 次 `Tab` 键唤醒/隐藏。

### Changed
- `FileBrowser` 重构为 `LabelGroup`，并新增 `PathInputField`。
- `FileBrowser` 原型改为外部 CSS/JS 动态渲染。
- UXML 加载工具重构，并新增 `FileBrowser` 窗口。

## v0.0.16

- 函数静态化。

## v0.0.15

- 优化 Singleton。
- 优化 MonoSingleton。

## v0.0.14

- MonoSingleton 允许自定义名称。
- 新增 README 文档。

## v0.0.13

- 新增资源路径脚本生成器。

## v0.0.12

- 移动 alias 路径，实现 ForgeMetaDatabase。

## v0.0.11

- 修复 Unity GUI 报错问题。

## v0.0.10

- 等第一帧渲染完全结束后再自动安装 Harmony。

## v0.0.9

- 新增泛型事件中心。

## v0.0.8

- 新增项目浏览器资源别名系统（ProjectBrowserAlias）。
- 新增 GitHub Actions 自动化发布流程。
- 通过分支清理优化发布流程。

## v0.0.6

- 添加 CHANGELOG.md 的 meta 文件。

## v0.0.5

- 新增版本回滚保护（version-guard）CI。

## v0.0.4

- Singleton / MonoSingleton 新增 `IsInitialized`、`HasInstance`、`IsDestroying` 生命周期标志。
- 移除已禁用的 AndroidX Core AAR 文件。

## v0.0.3

- 新增 Android 诊断日志、保活、通知支持。
- 新增 Singleton 泛型类。
- 新增编辑器文件选择工具窗口（UXML/USS）。
- 新增 Newtonsoft.Json 的 Vector3Converter。
- 新增 GIF 转序列帧工具与 Built-in/URP UI Shader 模板。
- 重构 DepotSettingsProvider。
- 优化版本解析逻辑。
- 示例目录结构标准化。

## v0.0.2

- 完成 Depot 包命名、版本号、作者与发布流程的初始对齐。
