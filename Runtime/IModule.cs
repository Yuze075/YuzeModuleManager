namespace YuzeToolkit.Framework.ModuleManager
{
    public interface IModule
    {
        /// <summary>
        /// 模块创建时的构造函数
        /// </summary>
        /// <param name="createParam">传入的构造参数</param>
        public void OnCreate(object createParam);
        /// <summary>
        /// 模块的更新函数，更新模式取决初始化设置
        /// </summary>
        public void OnUpdate();
        /// <summary>
        /// 模块的销毁函数
        /// </summary>
        public void OnDestroy();
    }
}