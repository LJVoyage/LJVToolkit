# LJV_Toolkit

**LJV_Toolkit** 是一个为 Unity 项目准备的通用工具包，封装了常用的工具函数、扩展方法与编辑器扩展。

它的设计目标是让项目开发更高效、更规范、更易维护。

---


## 🧠 命名空间规范

Runtime 代码：
```csharp
namespace LJVoyage.Toolkit.Runtime.Utilities
{
    public static class MathUtil
    {
        public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
        {
            return (value - inMin) / (inMax - inMin) * (outMax - outMin) + outMin;
        }
    }
}
```

##  安装方法

添加 scoped registries
{
  "name": "OpenUPM",
  "url": "https://package.openupm.com",
  "scopes": [
    "com.ljvoyage"
  ]
}
