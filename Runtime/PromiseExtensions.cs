using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.Core;

namespace AceLand.TaskUtils
{
    public static class PromiseExtensions
    {
        private static CancellationToken ApplicationAliveToken => 
            PromiseHelper.AliveSystem.ApplicationAliveTokenSource.Token;
        
        public static void StartCoroutine(this IEnumerator enumerator) =>
            Promise.Dispatcher.StartCoroutine(enumerator);
        public static Promise StartCoroutineAsTask(this IEnumerator enumerator) =>
            Promise.Dispatcher.StartCoroutineAsTask(enumerator);
        
        public static Promise WhenAll(this Promise[] promises) =>
            Task.WhenAll(promises.Select(promise => promise.AsTask()).ToArray());
        public static Promise<T[]> WhenAll<T>(this Promise<T>[] promises) =>
            Task.WhenAll(promises.Select(p => p.AsTask()).ToArray());
        public static Promise WhenAll(this List<Promise> promises) =>
            Task.WhenAll(promises.Select(promise => promise.AsTask()).ToArray());
        public static Promise<T[]> WhenAll<T>(this List<Promise<T>> promises) =>
            Task.WhenAll(promises.Select(p => p.AsTask()).ToArray());

        public static Task AsTask(this Promise promise) => promise.TaskCompletionSource.Task;
        public static Task<T> AsTask<T>(this Promise<T> promise) => promise.TaskCompletionSource.Task;
        
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
