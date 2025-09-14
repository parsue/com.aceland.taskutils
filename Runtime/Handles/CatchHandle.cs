using System;
using System.Collections.Generic;
using AceLand.Library.Disposable;
using UnityEngine;

namespace AceLand.TaskUtils.Handles
{
    internal class CatchHandle : DisposableObject
    {
        public CatchHandle()
        {
            var handler = new Handler<Exception>();
            _handlers[typeof(Exception)] = handler;
        }
        
        protected override void DisposeManagedResources()
        {
            foreach (var handler in _handlers.Values)
                handler.Dispose();
            
            _handlers.Clear();
        }

        private readonly Dictionary<Type, IHandler> _handlers = new();

        public void Invoke(Exception exception)
        {
            if (Disposed) return;
            
            var runtimeType = exception.GetType();

            if (TryGet(runtimeType, out var handler))
            {
                InvokeOnHandler(handler, exception);
                return;
            }

            var baseType = runtimeType.BaseType;
            while (baseType != null && typeof(Exception).IsAssignableFrom(baseType))
            {
                if (TryGet(baseType, out var baseHandler))
                {
                    InvokeOnHandler(baseHandler, exception);
                    return;
                }

                baseType = baseType.BaseType;
            }

            if (TryGet(typeof(Exception), out var defaultHandler))
                InvokeOnHandler(defaultHandler, exception);
            else
                Debug.LogWarning("Promise Catch Error: No handler found for Exception");
        }

        public T GetException<T>() where T : Exception
        {
            return TryGet<T>(out var handler)
                ? handler.Exception
                : null;
        }

        public void AddHandler<T>(Action<T> handler) where T : Exception
        {
            var handle = GetOrAdd<T>();
            handle.AddHandler(handler);
            Update<T>(handle);
        }
        
        private static void InvokeOnHandler(IHandler handler, Exception ex)
        {
            var handlerType = handler.GetType();

            var methods = handlerType.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            System.Reflection.MethodInfo best = null;

            foreach (var m in methods)
            {
                if (m.Name != "Invoke") continue;
                var ps = m.GetParameters();
                if (ps.Length != 1) continue;

                var pType = ps[0].ParameterType;
                if (pType == ex.GetType())
                {
                    best = m;
                    break;
                }
                if (pType.IsAssignableFrom(ex.GetType()))
                {
                    best ??= m;
                }
            }

            if (best != null)
            {
                best.Invoke(handler, new object[] { ex });
                return;
            }

            var fallback = handlerType.GetMethod("Invoke", new[] { typeof(Exception) });
            fallback?.Invoke(handler, new object[] { ex });
        }
        
        private bool TryGet(Type type, out IHandler handler)
        {
            return _handlers.TryGetValue(type, out handler);
        }

        private bool TryGet<T>(out Handler<T> handle) where T : Exception
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var handler))
            {
                handle = null;
                return false;
            }
            
            handle = handler as Handler<T>;
            return true;
        }
        
        private Handler<T> GetOrAdd<T>() where T : Exception
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var handler))
                return handler as Handler<T>;
            
            var handle = new Handler<T>();
            _handlers.Add(type, handle);
            return handle;
        }
        
        private void Update<T>(IHandler handler) where T : Exception
        {
            var type = typeof(T);
            _handlers[type] = handler;
        }

        private interface IHandler
        {
            void Dispose();
        }

        private sealed class Handler<T> : DisposableObject, IHandler
            where T : Exception
        {
            
            protected override void DisposeManagedResources()
            {
                Action = null;
            }

            public T Exception { get; private set; }
            private event Action<T> Action;

            public void Invoke(T exception)
            {
                Exception = exception;
                Action?.EnqueueToDispatcher(exception);
            }
            
            public void AddHandler(Action<T> handler)
            {
                Action += handler;
            }
        }
    }
}