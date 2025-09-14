using System;
using System.Collections;
using System.Threading.Tasks;
using AceLand.PlayerLoopHack;
using AceLand.TaskUtils.Mono;
using AceLand.TaskUtils.PlayerLoopSystems;

namespace AceLand.TaskUtils
{
    public static class PromiseExtensions
    {
        public static void RunCoroutine(this IEnumerator coroutine) =>
            PromiseAgent.CoroutineAgent(coroutine);
        
        public static void EnqueueToDispatcher(this Action action, PlayerLoopState state = PlayerLoopState.Initialization) =>
            UnityMainThreadDispatchers.Enqueue(action, state);
        public static void EnqueueToDispatcher<T>(this Action<T> action, T arg, PlayerLoopState state = PlayerLoopState.Initialization) =>
            UnityMainThreadDispatchers.Enqueue(action, arg, state);
        
        public static Task AsTask(this IEnumerator enumerator) =>
            PromiseAgent.RunCoroutine(enumerator);
        
        public static Promise Then(this Task task, Action onSuccess) =>
            Promise.Create<Exception>(task, thenAction: onSuccess);
        public static Promise Then(this Task task, Func<Task> onSuccess) =>
            Promise.Create<Exception>(task, thenTask: onSuccess);
        public static Promise<T> Then<T>(this Task<T> task, Action<T> onSuccess) =>
            Promise<T>.Create<Exception>(task, thenAction: onSuccess);
        public static Promise<T> Then<T>(this Task<T> task, Func<T, Task> onSuccess) =>
            Promise<T>.Create<Exception>(task, thenTask: onSuccess);
        
        public static Promise Catch(this Task task, Action<Exception> onError) =>
            Promise.Create(task, catchAction: onError);
        public static Promise Catch<T>(this Task task, Action<T> onError)
            where T : Exception=>
            Promise.Create(task, catchAction: onError);
        
        public static Promise Final(this Task task, Action onFinal) =>
            Promise.Create<Exception>(task, finalAction: onFinal);

        public static void Cancel(this Promise[] promises)
        {
            foreach (var promise in promises)
                promise.Cancel();
        }

        public static void Cancel<T>(this Promise<T>[] promises)
        {
            foreach (var promise in promises)
                promise.Cancel();
        }

        public static void Dispose(this Promise[] promises)
        {
            foreach (var promise in promises)
                promise.Dispose();
        }

        public static void Dispose<T>(this Promise<T>[] promises)
        {
            foreach (var promise in promises)
                promise.Dispose();
        }
    }
}
