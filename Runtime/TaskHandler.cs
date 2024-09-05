using System;
using System.Threading;
using AceLand.Library.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AceLand.TasksUtils
{
    public static class TaskHandler
    {
        private const int MAX_THREAD_POOL_SIZE = 1000;
        
        public static CancellationToken ApplicationAliveToken => _applicationAliveTokenSource.Token;
        public static int TasksInThreadPool => _tasksInThreadPool;
        public static int TasksInMainPool => _tasksInMainPool;

        private static CancellationTokenSource _applicationAliveTokenSource;
        private static event Action OnApplicationQuit;


        private static int _tasksInThreadPool = 0;
        private static int _tasksInMainPool = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initial()
        {
            Debug.Log("Task Handler is Active");
            Debug.Log($"Max Pool Size: {MAX_THREAD_POOL_SIZE}");
            TaskPool.SetMaxPoolSize(MAX_THREAD_POOL_SIZE);
            _applicationAliveTokenSource = new CancellationTokenSource();
            CreateMultiTask(OnApplicationEnd);
        }

        private static async UniTask OnApplicationEnd()
        {
            await UniTask.WaitUntil(() => !Application.isPlaying);
            OnApplicationQuit?.Invoke();
            _applicationAliveTokenSource?.Cancel();
            _applicationAliveTokenSource?.Dispose();
        }

        public static void AddApplicationQuitListener(Action listener) => OnApplicationQuit += listener;
        public static void RemoveApplicationQuitListener(Action listener) => OnApplicationQuit -= listener;

        public static void AddCustomMainTask() => _tasksInMainPool++;
        public static void RemoveCustomMainTask() => _tasksInMainPool--;
        public static void AddCustomMultiTask() => _tasksInThreadPool++;
        public static void RemoveCustomMultiTask() => _tasksInThreadPool--;

        public static UniTask CreateMultiTask(Func<UniTask> action)
        {
            var caller = action.GetOwnerName();
            var actionName = action.Method.Name;
            Debug.Log($"Create Task - {actionName} {caller}");
#if UNITY_WEBGL
            return UniTask.Create(async () =>
            {
                tasksInMainPool++;
#else
            return UniTask.RunOnThreadPool(async () =>
            {
                _tasksInThreadPool++;
#endif
                await action.Invoke();
#if UNITY_WEBGL
                tasksInMainPool--;
#else
                _tasksInThreadPool--;
#endif
                Debug.Log($"Cancel Task - {actionName} {caller}");
            });
        }

        public static CancellationTokenSource CreateMultiTask(Action action, CycleTiming timing, bool logging = true)
        {
            CreateTaskData(action, 
                out var caller, 
                out var actionName, 
                out var isMono, 
                out var transform, 
                out var tokenSource, 
                out var token, 
                out var quitToken);
            if (logging) Debug.Log($"Create Multi Task - {actionName} {timing.ToString()} {caller}");
#if UNITY_WEBGL
            UniTask.Create(async () =>
            {
                tasksInMainPool++;
#else
            UniTask.RunOnThreadPool(async () =>
            {
                _tasksInThreadPool++;
#endif
                if (isMono) await UniTask.Yield();
                while (!token.IsCancellationRequested && !quitToken.IsCancellationRequested && (!isMono || transform != null))
                {
                    action?.Invoke();
                    await GetAwaitable(timing, token);
                }
#if UNITY_WEBGL
                tasksInMainPool--;
#else
                _tasksInThreadPool--;
#endif
                if (logging) Debug.Log($"Cancel Multi Task - {actionName} {caller}");
                tokenSource?.Cancel();
                tokenSource?.Dispose();
            });
            return tokenSource;
        }

        public static UniTask CreateMainTask(Func<UniTask> action)
        {
            var caller = action.GetOwnerName();
            var actionName = action.Method.Name;
            Debug.Log($"Create Task - {actionName} {caller}");
            return UniTask.Create(async () =>
            {
                _tasksInMainPool++;
                await action.Invoke();
                _tasksInMainPool--;
                Debug.Log($"Cancel Task - {actionName} {caller}");
            });
        }

        public static CancellationTokenSource CreateMainTask(Action action, CycleTiming timing, bool logging = true)
        {
            CreateTaskData(action,
                out var caller,
                out var actionName,
                out var isMono,
                out var transform,
                out var tokenSource,
                out var token,
                out var quitToken);
            if (logging) Debug.Log($"Create Main Task - {actionName} {timing.ToString()} {caller}");
            UniTask.Create(async () =>
            {
                _tasksInMainPool++;
                if (isMono) await UniTask.Yield();
                while (!token.IsCancellationRequested && !quitToken.IsCancellationRequested && (!isMono || transform != null))
                {
                    action?.Invoke();
                    await GetAwaitable(timing, token);
                }
                if (logging) Debug.Log($"Cancel Main Task - {actionName} {caller}");
                _tasksInMainPool--;
                tokenSource?.Cancel();
                tokenSource?.Dispose();
            });
            return tokenSource;
        }

        private static void CreateTaskData(Action action, out string caller, out string actionName, out bool isMono, out Transform transform, out CancellationTokenSource tokenSource, out CancellationToken token, out CancellationToken quitToken)
        {
            caller = action.GetOwnerName();
            actionName = action.Method.Name;
            tokenSource = new();
            token = tokenSource.Token;
            quitToken = ApplicationAliveToken;
            isMono = TaskHelper.TryGetMono(action.Target, out var mono);
            transform = mono == null ? null : mono.transform;
        }

        private static UniTask GetAwaitable(CycleTiming timing, CancellationToken token, bool cancelImmediately = false)
        {
            return timing switch
            {
                CycleTiming.Initialization => UniTask.Yield(PlayerLoopTiming.Initialization, token, cancelImmediately),
                CycleTiming.LastInitialization => UniTask.Yield(PlayerLoopTiming.LastInitialization, token, cancelImmediately),
                CycleTiming.EarlyUpdate => UniTask.Yield(PlayerLoopTiming.EarlyUpdate, token, cancelImmediately),
                CycleTiming.LastEarlyUpdate => UniTask.Yield(PlayerLoopTiming.LastEarlyUpdate, token, cancelImmediately),
                CycleTiming.FixedUpdate => UniTask.Yield(PlayerLoopTiming.FixedUpdate, token, cancelImmediately),
                CycleTiming.LastFixedUpdate => UniTask.Yield(PlayerLoopTiming.LastFixedUpdate, token, cancelImmediately),
                CycleTiming.PreUpdate => UniTask.Yield(PlayerLoopTiming.PreUpdate, token, cancelImmediately),
                CycleTiming.LastPreUpdate => UniTask.Yield(PlayerLoopTiming.LastPreUpdate, token, cancelImmediately),
                CycleTiming.Update => UniTask.Yield(PlayerLoopTiming.Update, token, cancelImmediately),
                CycleTiming.LastUpdate => UniTask.Yield(PlayerLoopTiming.LastUpdate, token, cancelImmediately),
                CycleTiming.PreLateUpdate => UniTask.Yield(PlayerLoopTiming.PreLateUpdate, token, cancelImmediately),
                CycleTiming.LastPreLateUpdate => UniTask.Yield(PlayerLoopTiming.LastPreLateUpdate, token, cancelImmediately),
                CycleTiming.PostLateUpdate => UniTask.Yield(PlayerLoopTiming.PostLateUpdate, token, cancelImmediately),
                CycleTiming.LastPostLateUpdate => UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate, token, cancelImmediately),
                CycleTiming.TimeUpdate => UniTask.Yield(PlayerLoopTiming.TimeUpdate, token, cancelImmediately),
                CycleTiming.LastTimeUpdate => UniTask.Yield(PlayerLoopTiming.LastTimeUpdate, token, cancelImmediately),
                CycleTiming.EndOfFrame => UniTask.WaitForEndOfFrame(token),
                CycleTiming.NotSpecify => UniTask.Yield(token, cancelImmediately),
                _ => UniTask.Yield(PlayerLoopTiming.Update, token, cancelImmediately),
            };
        }
    }
}
