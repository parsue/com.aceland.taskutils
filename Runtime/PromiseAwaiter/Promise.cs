using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.PlayerLoopSystems;
using AceLand.TaskUtils.PromiseAwaiter.Core;

namespace AceLand.TaskUtils.PromiseAwaiter
{
    public sealed class Promise : Awaiter<bool>
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
            OnFinal = null;
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }
        
        private Action OnSuccess { get; set; }
        private Func<Task> OnSuccessTask { get; set; }
        private Action<Exception> OnError { get; set; }
        private Action OnFinal { get; set; }
        
        private CancellationTokenSource _tokenSource;

        private bool _isSuccess;
        private bool _isFail;
        private Exception _exception;

        public Promise Then(Action onSuccess)
        {
            if (Disposed) return this;
            if (_isSuccess)
            {
                onSuccess?.Invoke();
                return this;
            }

            OnSuccess += onSuccess;
            return this;
        }
        
        public Promise Then(Func<Task> onSuccess)
        {
            if (Disposed || IsCompleted) return this;
            if (Disposed) return this;
            if (_isSuccess)
            {
                onSuccess?.Invoke();
                return this;
            }

            OnSuccessTask += onSuccess;
            return this;
        }
        
        public Promise Catch(Action<Exception> onError)
        {
            if (Disposed) return this;
            if (_isFail)
            {
                onError?.Invoke(_exception);
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
            var linkedToken = TaskHelper.LinkedOrApplicationAliveToken(_tokenSource,
                out var linkedTokenSource);

            task.ContinueWith(t =>
                {
                    if (t.IsCanceled)
                    {
                        TaskCompletionSource.TrySetCanceled();
                    }
                    else if (t.IsFaulted && t.Exception?.InnerExceptions.Count > 0)
                    {
                        if (t.Exception?.InnerExceptions.Count > 0)
                        {
                            foreach (var exception in t.Exception.InnerExceptions)
                            {
                                if (OnError is not null)
                                    UnityMainThreadDispatcher.Enqueue(() => OnError(exception));
                            }
                            _exception = t.Exception.InnerExceptions[0];
                        }
                        else
                        {
                            _exception = t.Exception;
                            UnityMainThreadDispatcher.Enqueue(() => OnError(_exception));
                        }

                        TaskCompletionSource.TrySetException(_exception ?? new Exception());
                        _isFail = true;
                    }
                    else if (t.IsCompletedSuccessfully || t.IsCompleted)
                    {
                        if (OnSuccess is not null)
                            UnityMainThreadDispatcher.Enqueue(OnSuccess);
                        
                        if (OnSuccessTask is not null)
                        {
                            var successTasks = OnSuccessTask();
                            
                            while (!linkedToken.IsCancellationRequested && !successTasks.IsCompleted)
                                Thread.Yield();
                        }
                        
                        TaskCompletionSource.TrySetResult(true);
                        _isSuccess = true;
                    }

                    IsCompleted = true;
                    if (OnFinal is not null)
                        UnityMainThreadDispatcher.Enqueue(OnFinal);
                    if (Continuation is not null)
                        UnityMainThreadDispatcher.Enqueue(Continuation);
                    linkedTokenSource?.Dispose();
                },
                cancellationToken: linkedToken
            );
        }

        public static Promise WhenAll(Promise[] promises) =>
            Task.WhenAll(promises.Select(promise => promise.AsTask()).ToArray());
        public static Promise<T[]> WhenAll<T>(Promise<T>[] promises) =>
            Task.WhenAll(promises.Select(p => p.AsTask()).ToArray());

        internal Task AsTask() => TaskCompletionSource.Task;
        public static implicit operator Promise(Task task) => new(task);
        public static implicit operator Task(Promise promise) => promise.AsTask();
    }
}
