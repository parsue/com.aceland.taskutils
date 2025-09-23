﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.Handles;
using AceLand.TaskUtils.Mono;
using AceLand.TaskUtils.PlayerLoopSystems;

namespace AceLand.TaskUtils
{
    public sealed partial class Promise
    {
        private static readonly PromiseDispatcher promiseDispatcher = new();

        internal static void PromiseAgentReady(PromiseAgent agent, bool ready)
        {
            promiseDispatcher.PromiseAgent = agent;
            promiseDispatcher.Ready = ready;
        } 
        
        public static IPromiseDispatcher Dispatcher => promiseDispatcher;
        
        public static CancellationToken ApplicationAliveToken => 
            ApplicationAliveSystem.ApplicationAliveTokenSource.Token;

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

        public static Promise WaitForSeconds(float seconds)
        {
            return Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(seconds), ApplicationAliveToken);
                },
                ApplicationAliveToken
            );
        }

        public static Promise WaitUntil(Func<bool> condition)
        {
            return Task.Run(async () =>
                {
                    while (!condition() && !ApplicationAliveToken.IsCancellationRequested)
                        await Task.Delay(50, ApplicationAliveToken);
                },
                ApplicationAliveToken
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
            Task.Run(action, ApplicationAliveToken);
        public static Promise Run<T>(Action<T> action, T arg) =>
            Task.Run(() => action(arg), ApplicationAliveToken);
        public static Promise<T> Run<T>(Func<T> action) =>
            Task.Run(action, ApplicationAliveToken);
        public static Promise<T> Run<T, TArg>(Func<TArg, T> action, TArg arg) =>
            Task.Run(() => action(arg), ApplicationAliveToken);
        public static Promise Run(Func<Task> action) =>
            Task.Run(async () => await action(), ApplicationAliveToken);
        public static Promise<T> Run<T>(Func<Task<T>> action) =>
            Task.Run(async () => await action(), ApplicationAliveToken);
        public static Promise<T> Run<T, TArg>(Func<TArg, Task<T>> action, TArg arg) =>
            Task.Run(async () => await action(arg), ApplicationAliveToken);
        
        public static void AddApplicationQuitListener(Action listener) => 
            ApplicationAliveSystem.OnApplicationQuit += listener;
        public static void RemoveApplicationQuitListener(Action listener) => 
            ApplicationAliveSystem.OnApplicationQuit -= listener;
    }
}