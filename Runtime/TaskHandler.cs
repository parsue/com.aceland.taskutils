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

        public static CancellationToken LinkedOrApplicationAliveToken(CancellationTokenSource tokenSource,
            out CancellationTokenSource linkedTokenSource)
        {
            if (tokenSource == null)
            {
                linkedTokenSource = null;
                return ApplicationAliveToken;
            }
            
            linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                tokenSource.Token,
                ApplicationAliveToken
            );
            return linkedTokenSource.Token;
        }

        private static CancellationTokenSource _applicationAliveTokenSource;
        private static event Action OnApplicationQuit;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initial()
        {
            Debug.Log("Task Handler is Active");
            _applicationAliveTokenSource = new CancellationTokenSource();
            ApplicationAliveTask()
                .Catch(Debug.LogError);
        }

        private static async Task ApplicationAliveTask()
        {
            Debug.Log("Application Alive Task is running ...");
            while (Application.isPlaying)
                await Task.Yield();
            OnApplicationEnd();
        }

        private static void OnApplicationEnd()
        {
            _applicationAliveTokenSource?.Cancel();
            _applicationAliveTokenSource?.Dispose();
            OnApplicationQuit?.Invoke();
            Debug.Log("Application End");
        }

        public static void AddApplicationQuitListener(Action listener) => OnApplicationQuit += listener;
        public static void RemoveApplicationQuitListener(Action listener) => OnApplicationQuit -= listener;
    }
}
