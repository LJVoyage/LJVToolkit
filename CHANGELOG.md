## [1.0.0] - 2025-10-21

### 🎉 首次发布 (Initial Release)

#### 🧩 新增内容
- **通用工具结构搭建**
    - 创建 `Runtime/` 与 `Editor/` 文件夹结构。
    - 规范命名空间为：
        - `LJVoyage.Toolkit.Runtime.*`
        - `LJVoyage.Toolkit.Editor.*`
    - 支持以 Unity Package (UPM) 形式引用。

#### 🧮 Runtime 模块
- **MathUtil**
    - 新增 `Remap()`、`Clamp()`、`LerpUnclamped()` 等常用数学函数。

#### 🧠 Editor 模块
- **EditorMenuUtil**
- **Editor 文件结构**
    - 支持未来扩展 `Drawers`、`Windows`、`PropertyAttributes` 模块。

#### 📘 文档与版本控制
- 新增 `README.md`
    - 包含工具包概述、命名空间规范与使用示例。
- 新增 `CHANGELOG.md`
    - 记录更新历史。
- 确立版本号：`v1.0.0`。

---

## 🔮 未来计划
- [ ] 添加 `UIUtil`、`ColorUtil` 模块。
- [ ] 提供 `LogFilter` 日志过滤器与彩色调试控制台。
- [ ] 添加 `PrefabTool` 编辑器批处理工具。
- [ ] 增强 `FileUtil`，支持二进制文件与图像保存。
- [ ] 发布 `v1.1.0` 次版本更新。

---