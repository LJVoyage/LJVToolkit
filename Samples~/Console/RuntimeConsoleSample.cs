using UnityEngine;
using VoyageForge.Depot.Runtime.Console;

namespace VoyageForge.Depot.Samples.Console
{
    /// <summary>
    /// 运行时控制台示例引导器。
    ///
    /// 演示推荐用法：
    /// 1. 在 [RuntimeInitializeOnLoadMethod] 中调用 RuntimeConsole.Initialize() 完成初始化；
    /// 2. 初始化只会创建实例、构建面板，但不会显示；
    /// 3. 运行后连按 3 次 Tab 键唤醒/隐藏控制台。
    /// </summary>
    public static class RuntimeConsoleBootstrap
    {
        /// <summary>
        /// 应用启动后自动执行，完成运行时控制台的初始化（默认隐藏）。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeConsole()
        {
            // 创建单例并完成面板构建；默认不显示，等待三连 Tab 唤醒
            RuntimeConsole.Initialize();

            // 打印一条日志，唤醒控制台后即可看到，用于验证日志捕获链路
            Debug.Log("Depot 运行时控制台已初始化（隐藏中），连按 3 次 Tab 键唤醒。");
        }
    }
}
