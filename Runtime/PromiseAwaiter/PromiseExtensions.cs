using System;
using System.Threading.Tasks;

namespace AceLand.TaskUtils.PromiseAwaiter
{
    public static class PromiseExtensions
    {
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
    }
}
