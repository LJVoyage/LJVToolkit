using UnityEngine.Scripting;

namespace VoyageForge.Depot.Runtime.Console
{
    /// <summary>
    /// 内置命令：clear —— 清空控制台日志缓冲与各级别计数。
    /// 标注 [Preserve] 防止 IL2CPP 代码剥离，确保反射自动发现能注册本命令。
    /// </summary>
    [Preserve]
    public sealed class ClearCommand : ConsoleCommand
    {
        /// <inheritdoc />
        public override string Name => "clear";

        /// <inheritdoc />
        public override string Description => "清空所有日志";

        /// <inheritdoc />
        public override string Usage => "clear";

        /// <summary>
        /// 执行清空操作：调用控制台单例的 <see cref="RuntimeConsole.Clear"/>。
        /// </summary>
        /// <param name="args">本命令忽略参数。</param>
        public override void Execute(string[] args)
        {
            // 使用空条件访问：即使单例尚未创建也不会抛异常
            RuntimeConsole.Instance?.Clear();
        }
    }
}
