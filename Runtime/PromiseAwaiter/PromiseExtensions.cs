using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace AceLand.TaskUtils.PromiseAwaiter
{
    public static class PromiseExtensions
    {
        public static Promise<T> Then<T>(this Task<T> task, Action<T> onSuccess) =>
            new(onSuccess, task.AsUniTask());
        public static Promise<T> Then<T>(this Task<T> task, Func<T, Task> onSuccess) =>
            new(onSuccess, task.AsUniTask());
        public static Promise<T> Catch<T>(this Task<T> task, Action<Exception> onError) =>
            new(onError, task.AsUniTask());
        public static Promise<T> Final<T>(this Task<T> task, Action onFinal) =>
            new(onFinal, task.AsUniTask());
        public static Promise<T> OnCompleted<T>(this Task<T> task, Action onFinal) =>
            new(onFinal, task.AsUniTask());
        
        public static Promise Then(this Task task, Action onSuccess) =>
            new(onSuccess, task.AsUniTask(), false);
        public static Promise Then(this Task task, Func<Task> onSuccess) =>
            new(onSuccess, task.AsUniTask());
        public static Promise Catch(this Task task, Action<Exception> onError) =>
            new(onError, task.AsUniTask());
        public static Promise Final(this Task task, Action onFinal) =>
            new(onFinal, task.AsUniTask(), true);
        public static Promise OnCompleted(this Task task, Action onFinal) =>
            new(onFinal, task.AsUniTask(), true);
        
        public static Promise<T> Then<T>(this UniTask<T> task, Action<T> onSuccess) =>
            new(onSuccess, task);
        public static Promise<T> Then<T>(this UniTask<T> task, Func<T, Task> onSuccess) =>
            new(onSuccess, task);
        public static Promise<T> Catch<T>(this UniTask<T> task, Action<Exception> onError) =>
            new(onError, task);
        public static Promise<T> Final<T>(this UniTask<T> task, Action onFinal) =>
            new(onFinal, task);
        public static Promise<T> OnCompleted<T>(this UniTask<T> task, Action onFinal) =>
            new(onFinal, task);
        
        public static Promise Then(this UniTask task, Action onSuccess) =>
            new(onSuccess, task, false);
        public static Promise Then(this UniTask task, Func<Task> onSuccess) =>
            new(onSuccess, task);
        public static Promise Catch(this UniTask task, Action<Exception> onError) =>
            new(onError, task);
        public static Promise Final(this UniTask task, Action onFinal) =>
            new(onFinal, task, true);
        public static Promise OnCompleted(this UniTask task, Action onFinal) =>
            new(onFinal, task, true);
    }
}
