using System;
using UnityEngine;

namespace YuzeToolkit.Framework.ModuleManager
{
    /// <summary>
    /// 驱动模块更新的脚本
    /// </summary>
    public class ModuleDriver : MonoBehaviour
    {
        private static ModuleDriver _instance;

        public static ModuleDriver Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var obj = new GameObject($"__{nameof(ModuleDriver)}__");
                _instance = obj.AddComponent<ModuleDriver>();
                return _instance;
            }
        }
        
        public Action UpdateAction { get; internal set; }
        public Action FixedUpdateAction { get; internal set; }
        public Action LateUpdateAction { get; internal set; }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(this);
                Logger.Error($"{typeof(ModuleDriver)} instance already created.");
            }
            DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            UpdateAction?.Invoke();
        }

        private void FixedUpdate()
        {
            FixedUpdateAction?.Invoke();
        }

        private void LateUpdate()
        {
            LateUpdateAction?.Invoke();
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}