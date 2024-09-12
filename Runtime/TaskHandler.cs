using System;
using System.Threading;
using AceLand.TaskUtils.PromiseAwaiter;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AceLand.TaskUtils
{
    public static class TaskHandler
    {
        private const int MAX_THREAD_POOL_SIZE = 128;
        
        public static CancellationToken ApplicationAliveToken => _applicationAliveTokenSource.Token;

        private static CancellationTokenSource _applicationAliveTokenSource;
        private static event Action OnApplicationQuit;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initial()
        {
            Debug.Log("Task Handler is Active");
            Debug.Log($"Max Pool Size: {MAX_THREAD_POOL_SIZE}");
            TaskPool.SetMaxPoolSize(MAX_THREAD_POOL_SIZE);
            _applicationAliveTokenSource = new CancellationTokenSource();
            OnApplicationEnd().Catch(Debug.LogError);
        }

        private static async UniTask OnApplicationEnd()
        {
            Debug.Log("Application Alive Task is running ...");
            await UniTask.WaitUntil(() => !Application.isPlaying);
            OnApplicationQuit?.Invoke();
            _applicationAliveTokenSource?.Cancel();
            _applicationAliveTokenSource?.Dispose();
        }

        public static void AddApplicationQuitListener(Action listener) => OnApplicationQuit += listener;
        public static void RemoveApplicationQuitListener(Action listener) => OnApplicationQuit -= listener;
    }
}
