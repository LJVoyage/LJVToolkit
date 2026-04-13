# Depot

## 简介
Depot 是 VoyageForge 的基础工具仓库，定位为 Unity 开发过程中的公共物资库。

它收拢项目中高频复用的运行时工具、编辑器辅助能力和通用扩展，让常用能力有统一的存放位置、命名方式和维护边界。

## 当前内容
- 运行时通用能力，例如数学工具、单例基类、场景引用与状态机辅助。
- 编辑器辅助能力，例如只读属性绘制、Project Settings 配置、构建前自动版本处理与启动项控制。
- 面向包开发的基础设施，例如程序集划分、打包元数据与工作流配置。

## 目录说明
- `Runtime/Scripts/Utilities`
  Depot 的运行时通用工具目录。
- `Runtime/Scripts/Attributes`
  运行时可用的特性定义。
- `Editor/Scripts/Utilities`
  Depot 的编辑器工具与 Project Settings 相关实现。
- `Editor/Scripts/Inspector`
  自定义 Inspector 与编辑器界面相关实现。

## 设计目标
- 把零散的共用工具统一沉淀到一个稳定的仓库模块中。
- 尽量让运行时工具与编辑器工具边界清晰，便于裁剪与维护。
- 为其他包提供可复用的底层支持，而不是把通用能力散落到业务代码中。

## 命名说明
- `Depot` 代表仓库、补给站。
- 在整个 VoyageForge 体系中，Depot 更适合作为共用工具与底层能力的集中存放点。
- 新增工具时，优先判断它是否属于跨模块复用的公共能力；如果是，应优先沉淀到 Depot。
