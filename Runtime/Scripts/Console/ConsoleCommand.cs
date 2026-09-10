using UnityEngine.Scripting;

namespace VoyageForge.Depot.Runtime.Console
{
    /// <summary>
    /// 控制台命令抽象基类。
    ///
    /// 派生类只需实现 <see cref="Name"/> 与 <see cref="Execute"/>（可选重写
    /// <see cref="Description"/>），即可被 <see cref="RuntimeConsole"/> 通过反射
    /// 自动发现并注册，无需手动调用任何注册方法。
    ///
    /// 重要：派生类必须标注 [Preserve]（<see cref="PreserveAttribute"/>），
    /// 防止 IL2CPP 构建时因“没有显式代码引用”而被代码剥离，否则反射扫描无法发现该命令。
    /// </summary>
    [Preserve]
    public abstract class ConsoleCommand
    {
        /// <summary>
        /// 命令名（用户在命令输入框敲入的关键字，不区分大小写）。
        /// 必须全局唯一，否则注册时后注册者会被跳过。
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 命令的简短描述，用于 help 命令展示。默认返回空字符串。
        /// </summary>
        public virtual string Description => string.Empty;

        /// <summary>
        /// 命令用法（如 "log &lt;message&gt;"），用于 help 命令展示。默认等于命令名。
        /// </summary>
        public virtual string Usage => Name;

        /// <summary>
        /// 执行命令。
        /// </summary>
        /// <param name="args">空格分隔的参数数组（不含命令名本身，可能为空数组）。</param>
        public abstract void Execute(string[] args);
    }
}
