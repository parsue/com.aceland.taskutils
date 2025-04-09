using System;
using System.Collections;
using System.Threading.Tasks;
using AceLand.TaskUtils.Mono;
using AceLand.TaskUtils.PlayerLoopSystems;

namespace AceLand.TaskUtils
{
    public static class PromiseExtensions
    {
        public static void EnqueueToDispatcher(this Action action) =>
            UnityMainThreadDispatcher.Enqueue(action);
        
        public static Task AsTask(this IEnumerator enumerator) =>
            PromiseAgent.RunCoroutine(enumerator);
        
        public static Promise<T> Then<T>(this Task<T> task, Action<T> onSuccess) =>
            new(task, thenAction: onSuccess);
        public static Promise<T> Then<T>(this Task<T> task, Func<T, Task> onSuccess) =>
            new(task, thenTask: onSuccess);
        public static Promise<T> Catch<T>(this Task<T> task, Action<Exception> onError) =>
            new(task, catchAction: onError);
        public static Promise<T> Final<T>(this Task<T> task, Action onFinal) =>
            new(task, finalAction: onFinal);
        
        public static Promise Then(this Task task, Action onSuccess) =>
            new(task, thenAction: onSuccess);
        public static Promise Then(this Task task, Func<Task> onSuccess) =>
            new(task, thenTask: onSuccess);
        public static Promise Catch(this Task task, Action<Exception> onError) =>
            new(task, catchAction: onError);
        public static Promise Final(this Task task, Action onFinal) =>
            new(task, finalAction: onFinal);

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
