using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.Core;
using AceLand.TaskUtils.PlayerLoopSystems;

namespace AceLand.TaskUtils
{
    public sealed partial class Promise : Awaiter<bool>
    {
        internal Promise(Task task,
            Action thenAction = null, Func<Task> thenTask = null,
            Action<Exception> catchAction = null,
            Action finalAction = null)
        {
            if (thenAction is not null) Then(thenAction);
            if (thenTask is not null) Then(thenTask);
            if (catchAction is not null) Catch(catchAction);
            if (finalAction is not null) Final(finalAction);
            HandleTask(task);
        }
        
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            Cancel();
        }

        public override void Cancel()
        {
            base.Cancel();
            OnSuccess = null;
            OnSuccessTask = null;
            OnError = null;
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }
        
        private Action OnSuccess { get; set; }
        private Func<Task> OnSuccessTask { get; set; }
        private Action<Exception> OnError { get; set; }
        private Action OnFinal { get; set; }
        
        private CancellationTokenSource _tokenSource;

        public Promise Then(Action onSuccess)
        {
            if (Disposed || IsCanceled) return this;
            
            if (IsSuccess)
            {
                onSuccess?.Invoke();
                return this;
            }

            OnSuccess += onSuccess;
            return this;
        }
        
        public Promise Then(Func<Task> onSuccess)
        {
            if (Disposed || IsCanceled) return this;
            
            if (IsSuccess)
            {
                onSuccess?.Invoke();
                return this;
            }

            OnSuccessTask += onSuccess;
            return this;
        }
        
        public Promise Catch(Action<Exception> onError)
        {
            if (Disposed || IsCanceled) return this;
            
            if (IsFault)
            {
                onError?.Invoke(Exception);
                return this;
            }

            OnError += onError;
            return this;
        }
        
        public Promise Final(Action onFinal)
        {
            if (Disposed) return this;

            if (IsCompleted)
            {
                onFinal?.Invoke();
                return this;
            }

            OnFinal += onFinal;
            return this;
        }

        private void HandleTask(Task task)
        {
            _tokenSource = new CancellationTokenSource();
            var linkedToken = LinkedOrApplicationAliveToken(_tokenSource,
                out var linkedTokenSource);

            task.ContinueWith(t =>
                {
                    if (t.IsCanceled)
                    {
                        Cancel();
                    }
                    else if (t.IsFaulted)
                    {
                        OnException(t);
                        Fault();
                    }
                    else if (t.IsCompletedSuccessfully || t.IsCompleted)
                    {
                        Result = true;
                        
                        if (OnSuccessTask is not null)
                        {
                            var successTasks = OnSuccessTask();
                            
                            while (!linkedToken.IsCancellationRequested && !successTasks.IsCompleted)
                                Thread.Yield();
                            
                            if (successTasks.IsFaulted && successTasks.Exception?.InnerExceptions.Count > 0)
                            {
                                OnException(successTasks);
                                Fault();
                            }
                        }

                        if (!IsFault)
                        {
                            if (OnSuccess is not null)
                                UnityMainThreadDispatcher.Enqueue(OnSuccess);
                            
                            Success();
                        }
                    }

                    OnFinalize(linkedTokenSource);
                },
                cancellationToken: linkedToken
            );
        }

        private void OnException(Task t)
        {
            if (t.Exception?.InnerExceptions.Count > 0)
            {
                foreach (var exception in t.Exception.InnerExceptions)
                {
                    if (OnError is not null)
                        UnityMainThreadDispatcher.Enqueue(() => OnError(exception));
                }
                Exception = t.Exception.InnerExceptions[0];
            }
            else
            {
                Exception = t.Exception ?? new Exception("unknown exception");
                UnityMainThreadDispatcher.Enqueue(() => OnError(Exception));
            }
        }

        private void OnFinalize(CancellationTokenSource linkedTokenSource)
        {
            IsCompleted = true;

            if (Disposed) return;
            
            if (OnFinal is not null)
                UnityMainThreadDispatcher.Enqueue(OnFinal);
            if (Continuation is not null)
                UnityMainThreadDispatcher.Enqueue(Continuation);
        }

        public static Promise WhenAll(Promise[] promises) =>
            Task.WhenAll(promises.Select(promise => promise.AsTask()).ToArray());
        public static Promise<T[]> WhenAll<T>(Promise<T>[] promises) =>
            Task.WhenAll(promises.Select(p => p.AsTask()).ToArray());

        public static Promise WhenAll(List<Promise> promises) =>
            Task.WhenAll(promises.Select(promise => promise.AsTask()).ToArray());
        public static Promise<T[]> WhenAll<T>(List<Promise<T>> promises) =>
            Task.WhenAll(promises.Select(p => p.AsTask()).ToArray());

        public static Task SafeRun(Action action) =>
            Task.Run(action);
        public static Task SafeRun(Func<Task> action) =>
            Task.Run(async () => await action.Invoke());

        internal Task AsTask() => TaskCompletionSource.Task;
        public static implicit operator Promise(Task task) => new(task);
        public static implicit operator Task(Promise promise) => promise.AsTask();
    }
}
