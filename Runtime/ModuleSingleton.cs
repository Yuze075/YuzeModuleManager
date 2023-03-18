namespace YuzeToolkit.Framework.ModuleManager
{
    /// <summary>
    /// 单例模块，方便调用
    /// </summary>
    /// <typeparam name="T">继承自<see cref="ModuleSingleton{T}"/>的类型，并且存在空构造函数<code>new()</code></typeparam>
    public abstract class ModuleSingleton<T> : IModule where T : ModuleSingleton<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    Logger.Error($"{typeof(T)} is not create.");
                return _instance;
            }
        }

        public virtual void OnCreate(object createParam)
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else
            {
                OnDestroy();
                Logger.Error($"{typeof(T)} instance already created.");
            }
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}