using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AceLand.Lifecycle;

namespace AceLand.TaskUtils
{
    public sealed partial class Promise
    {
        private static CancellationToken ApplicationAlive => LifecycleToken.ApplicationAlive;
        
        public static Promise WaitForSeconds(float seconds)
        {
            return Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(seconds), ApplicationAlive);
                },
                ApplicationAlive
            );
        }

        public static Promise WaitUntil(Func<bool> condition)
        {
            return Task.Run(async () =>
                {
                    while (!condition() && !ApplicationAlive.IsCancellationRequested)
                        await Task.Delay(50, ApplicationAlive);
                },
                ApplicationAlive
            );
        }

        public static Promise WhenAll(Promise[] promises) =>
            Task.WhenAll(promises.Select(promise => promise.AsTask()).ToArray());
        public static Promise<T[]> WhenAll<T>(Promise<T>[] promises) =>
            Task.WhenAll(promises.Select(p => p.AsTask()).ToArray());
        public static Promise WhenAll(List<Promise> promises) =>
            Task.WhenAll(promises.Select(promise => promise.AsTask()).ToArray());
        public static Promise<T[]> WhenAll<T>(List<Promise<T>> promises) =>
            Task.WhenAll(promises.Select(p => p.AsTask()).ToArray());

        public static Promise Run(Action action) =>
            Task.Run(action, ApplicationAlive);
        public static Promise Run<T>(Action<T> action, T arg) =>
            Task.Run(() => action(arg), ApplicationAlive);
        public static Promise<T> Run<T>(Func<T> action) =>
            Task.Run(action, ApplicationAlive);
        public static Promise<T> Run<T, TArg>(Func<TArg, T> action, TArg arg) =>
            Task.Run(() => action(arg), ApplicationAlive);
        public static Promise Run(Func<Task> action) =>
            Task.Run(async () => await action(), ApplicationAlive);
        public static Promise<T> Run<T>(Func<Task<T>> action) =>
            Task.Run(async () => await action(), ApplicationAlive);
        public static Promise<T> Run<T, TArg>(Func<TArg, Task<T>> action, TArg arg) =>
            Task.Run(async () => await action(arg), ApplicationAlive);
    }
}