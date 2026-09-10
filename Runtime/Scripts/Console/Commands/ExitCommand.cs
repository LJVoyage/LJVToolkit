using UnityEngine.Scripting;

namespace VoyageForge.Depot.Runtime.Console
{
    /// <summary>
    /// 内置命令：exit —— 退出 / 隐藏控制台。
    /// 标注 [Preserve] 防止 IL2CPP 代码剥离，确保反射自动发现能注册本命令。
    /// </summary>
    [Preserve]
    public sealed class ExitCommand : ConsoleCommand
    {
        /// <inheritdoc />
        public override string Name => "exit";

        /// <inheritdoc />
        public override string Description => "退出控制台";

        /// <inheritdoc />
        public override string Usage => "exit";

        /// <summary>
        /// 执行退出操作：隐藏控制台面板（等价于点击关闭按钮）。
        /// </summary>
        /// <param name="args">本命令忽略参数。</param>
        public override void Execute(string[] args)
        {
            // 隐藏控制台，使用空条件访问确保单例不存在时也不抛异常
            RuntimeConsole.Instance?.Hide();
        }
    }
}
