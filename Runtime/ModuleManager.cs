using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YuzeToolkit.Framework.ModuleManager
{
    [ExecuteAlways]
    public static class ModuleManager
    {
        public enum UpdateMode
        {
            Update,
            FixedUpdate,
            LateUpdate
        }

        /// <summary>
        /// 包装器，用于封装模块
        /// </summary>
        private class Wrapper
        {
            public IModule Module { get; }
            public int Priority { get; }
            public UpdateMode WrapperUpdateMode { get; }

            public Wrapper(IModule module, int priority, UpdateMode wrapperUpdateMode)
            {
                Module = module;
                Priority = priority;
                WrapperUpdateMode = wrapperUpdateMode;
            }
        }

        private static readonly List<Wrapper> Wrappers = new(100);

        private static ModuleDriver _moduleDriver;

        private static UpdateMode _defaultUpdateMode;
        private static bool _isInitialize;
        private static bool _isDirty;

        /// <summary>
        /// 初始化模块管理器
        /// </summary>
        /// <param name="defaultUpdateMode">设置默认的更新模式</param>
        public static void Initialize(UpdateMode defaultUpdateMode = UpdateMode.FixedUpdate)
        {
            if (_isInitialize)
            {
                Logger.Warning("[Module.Initialize]: Module already initialize");
                return;
            }

            // 创建驱动器
            _isInitialize = true;
            _moduleDriver = ModuleDriver.Instance;
            _defaultUpdateMode = defaultUpdateMode;

            // 绑定更新函数
            _moduleDriver.FixedUpdateAction += OnFixedUpdate;
            _moduleDriver.UpdateAction += OnUpdate;
            _moduleDriver.LateUpdateAction += OnLateUpdate;
        }


        /// <summary>
        /// 在<see cref="ModuleDriver.FixedUpdate"/>中更新
        /// </summary>
        private static void OnFixedUpdate()
        {
            SortWrappers();
            foreach (var wrapper in Wrappers.Where(wrapper => wrapper.WrapperUpdateMode == UpdateMode.FixedUpdate))
            {
                wrapper.Module.OnUpdate();
            }
        }

        /// <summary>
        /// 在<see cref="ModuleDriver.Update"/>中更新
        /// </summary>
        private static void OnUpdate()
        {
            SortWrappers();
            foreach (var wrapper in Wrappers.Where(wrapper => wrapper.WrapperUpdateMode == UpdateMode.Update))
            {
                wrapper.Module.OnUpdate();
            }
        }

        /// <summary>
        /// 在<see cref="ModuleDriver.LateUpdate"/>中更新
        /// </summary>
        private static void OnLateUpdate()
        {
            SortWrappers();
            foreach (var wrapper in Wrappers.Where(wrapper => wrapper.WrapperUpdateMode == UpdateMode.LateUpdate))
            {
                wrapper.Module.OnUpdate();
            }
        }

        /// <summary>
        /// 如果模块发生了变动对其重新进行排序
        /// </summary>
        private static void SortWrappers()
        {
            if (!_isDirty) return;
            _isDirty = false;
            Wrappers.Sort((left, right) =>
            {
                if (left.Priority > right.Priority) return -1;
                return left.Priority == right.Priority ? 0 : 1;
            });
        }

        /// <summary>
        /// 销毁所有模块
        /// </summary>
        public static void Destroy()
        {
            if (!_isInitialize) return;
            foreach (var wrapper in Wrappers)
            {
                wrapper.Module.OnDestroy();
            }

            Wrappers.Clear();

            _isInitialize = false;
            _isDirty = false;
            _moduleDriver = null;
        }

        /// <summary>
        /// 创建模块，设置为默认的更新模式<see cref="UpdateMode"/>
        /// </summary>
        /// <param name="priority">模块更新优先级，默认为0</param>
        /// <param name="createParam">模块创建时传入参数，默认为null</param>
        /// <typeparam name="T">继承自<see cref="ModuleSingleton{T}"/>的类型，并且存在空构造函数<code>new()</code></typeparam>
        /// <returns>返回创建的对象</returns>
        public static T CreateModule<T>(int priority = 0, object createParam = null) where T : ModuleSingleton<T>, new()
        {
            return CreateModule<T>(_defaultUpdateMode, priority, createParam);
        }

        /// <summary>
        /// 创建模块
        /// </summary>
        /// <param name="mode">设置更新模式</param>
        /// <param name="priority">模块更新优先级，默认为0</param>
        /// <param name="createParam">模块创建时传入参数，默认为null</param>
        /// <typeparam name="T">继承自<see cref="ModuleSingleton{T}"/>的类型，并且存在空构造函数<code>new()</code></typeparam>
        /// <returns>返回创建的对象</returns>
        public static T CreateModule<T>(UpdateMode mode, int priority = 0, object createParam = null)
            where T : ModuleSingleton<T>, new()
        {
            if (priority < 0)
            {
                Logger.Warning("[Module.CreateModule]: The priority can not be negative");
                priority = 0;
            }

            if (Contains<T>())
            {
                Logger.Warning($"[Module.CreateModule]: {typeof(T)} module is already existed");
                return null;
            }

            var module = new T();
            module.OnCreate(createParam);

            var wrapper = new Wrapper(module, priority, mode);
            Wrappers.Add(wrapper);

            _isDirty = true;
            return module;
        }

        /// <summary>
        /// 销毁对应模块
        /// </summary>
        /// <typeparam name="T">继承自<see cref="ModuleSingleton{T}"/>的类型，并且存在空构造函数<code>new()</code></typeparam>
        /// <returns>返回是否销毁成功</returns>
        public static bool DestroyModule<T>() where T : ModuleSingleton<T>, new()
        {
            var type = typeof(T);
            foreach (var wrapper in Wrappers.Where(wrapper => wrapper.Module.GetType() == type))
            {
                wrapper.Module.OnDestroy();
                Wrappers.Remove(wrapper);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 判断模块是否存在
        /// </summary>
        /// <typeparam name="T">继承自<see cref="ModuleSingleton{T}"/>的类型，并且存在空构造函数<code>new()</code></typeparam>
        /// <returns>返回是否存在</returns>
        public static bool Contains<T>() where T : ModuleSingleton<T>, new()
        {
            var type = typeof(T);
            return Wrappers.Any(wrapper => wrapper.Module.GetType() == type);
        }

        /// <summary>
        /// 获取到对应的模块，如果没有就返回null
        /// </summary>
        /// <typeparam name="T">继承自<see cref="ModuleSingleton{T}"/>的类型，并且存在空构造函数<code>new()</code></typeparam>
        /// <returns>返回获取到的模块</returns>
        public static T GetModule<T>() where T : ModuleSingleton<T>, new()
        {
            var type = typeof(T);
            foreach (var wrapper in Wrappers.Where(wrapper => wrapper.Module.GetType() == type))
            {
                return wrapper.Module as T;
            }

            Logger.Warning($"[Module.GetModule]: Not found {type} module");
            return null;
        }
    }
}