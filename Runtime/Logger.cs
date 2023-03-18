namespace YuzeToolkit.Framework.ModuleManager
{
    public static class Logger
    {
        public static void Log(string massage)
        {
#if LOGGER_SYSTEM
            LoggerSystem.LoggerSystem.Log(massage, new[] { "ModuleManager" });
#else
            UnityEngine.Debug.Log(massage);
#endif
        }

        public static void Warning(string massage)
        {
#if LOGGER_SYSTEM
            LoggerSystem.LoggerSystem.Warning(massage, new[] { "ModuleManager" });
#else
            UnityEngine.Debug.LogWarning(massage);
#endif
        }

        public static void Error(string massage)
        {
#if LOGGER_SYSTEM
            LoggerSystem.LoggerSystem.Error(massage, new[] { "ModuleManager" });
#else
            UnityEngine.Debug.LogError(massage);
#endif
        }
    }
}