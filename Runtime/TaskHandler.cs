using System;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.PromiseAwaiter;
using UnityEngine;

namespace AceLand.TaskUtils
{
    public static class TaskHandler
    {
        public static CancellationToken ApplicationAliveToken => _applicationAliveTokenSource.Token;

        private static CancellationTokenSource _applicationAliveTokenSource;
        private static event Action OnApplicationQuit;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initial()
        {
            Debug.Log("Task Handler is Active");
            _applicationAliveTokenSource = new CancellationTokenSource();
            ApplicationAliveTask()
                .Catch(Debug.LogError)
                .Final(OnApplicationEnd);
        }

        private static async Task ApplicationAliveTask()
        {
            Debug.Log("Application Alive Task is running ...");
            while (Application.isPlaying)
                await Task.Yield();
        }

        private static void OnApplicationEnd()
        {
            OnApplicationQuit?.Invoke();
            _applicationAliveTokenSource?.Cancel();
            _applicationAliveTokenSource?.Dispose();
        }

        public static void AddApplicationQuitListener(Action listener) => OnApplicationQuit += listener;
        public static void RemoveApplicationQuitListener(Action listener) => OnApplicationQuit -= listener;
    }
}
