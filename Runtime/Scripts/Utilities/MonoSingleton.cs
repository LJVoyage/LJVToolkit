using UnityEngine;

namespace LJVoyage.LJVToolkit.Runtime.Utilities
{
    /// <summary>
    /// 可继承的 MonoBehaviour 单例基类。
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

        private static readonly object _lock = new object();

        private static bool _applicationIsQuitting = false;

        /// <summary>获取单例实例。</summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[MonoSingleton] 已退出应用，不再创建 {typeof(T)} 单例。");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // 尝试在场景中查找现有实例
                        _instance = (T)FindObjectOfType(typeof(T));

                        // 如果没有，则创建新 GameObject
                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"[Singleton] {typeof(T)}";

                            // 默认设置为跨场景持久化
                            DontDestroyOnLoad(singletonObject);

                            Debug.Log($"[MonoSingleton] 创建 {typeof(T)} 单例。");
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>在 Awake 时检查重复实例</summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[MonoSingleton] 场景中已有 {typeof(T)} 实例，销毁重复实例。");
                Destroy(gameObject);
            }
        }

        /// <summary>应用退出时标记单例销毁</summary>
        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}