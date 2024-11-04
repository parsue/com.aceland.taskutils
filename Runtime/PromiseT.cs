using System;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.Core;
using AceLand.TaskUtils.PlayerLoopSystems;

namespace AceLand.TaskUtils
{
    public sealed class Promise<T> : Awaiter<T>
    {
        internal Promise(Task<T> task, 
            Action<T> thenAction = null, Func<T, Task> thenTask = null,
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

        private Action<T> OnSuccess { get; set; }
        private Func<T, Task> OnSuccessTask { get; set; }
        private Action<Exception> OnError { get; set; }
        private Action OnFinal { get; set; }
        
        private CancellationTokenSource _tokenSource;

        public Promise<T> Then(Action<T> onSuccess)
        {
            if (IsCanceled || Disposed) return this;
            
            if (IsSuccess)
            {
                onSuccess?.Invoke(Result);
                return this;
            }
            
            OnSuccess += onSuccess;
            return this;
        }
        
        public Promise<T> Then(Func<T, Task> onSuccess)
        {
            if (IsCanceled || Disposed) return this;
            
            if (IsSuccess)
            {
                onSuccess?.Invoke(Result);
                return this;
            }
            
            OnSuccessTask += onSuccess;
            return this;
        }
        
        public Promise<T> Catch(Action<Exception> onError)
        {
            if (IsCanceled || Disposed) return this;
            
            if (IsFault)
            {
                onError?.Invoke(Exception);
                return this;
            }
            
            OnError += onError;
            return this;
        }
        
        public Promise<T> Final(Action onFinal)
        {
            if (IsCanceled || Disposed) return this;
            
            if (IsCompleted)
            {
                onFinal?.Invoke();
                return this;
            }
            
            OnFinal += onFinal;
            return this;
        }

        private void HandleTask(Task<T> task)
        {
            _tokenSource = new CancellationTokenSource();
            var linkedToken = Promise.LinkedOrApplicationAliveToken(_tokenSource,
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
                        Result = t.Result;
                        
                        if (OnSuccessTask is not null)
                        {
                            var successTasks = OnSuccessTask(Result);
                            
                            while (!linkedToken.IsCancellationRequested && !successTasks.IsCompleted)
                                Thread.Yield();
                            
                            if (successTasks.IsFaulted)
                            {
                                OnException(successTasks);
                                Fault();
                            }
                        }

                        if (!IsFault)
                        {
                            if (OnSuccess is not null)
                                UnityMainThreadDispatcher.Enqueue(() => OnSuccess(Result));
                            
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
                UnityMainThreadDispatcher.Enqueue(() => OnError(t.Exception));
            }
        }

        private void OnFinalize(CancellationTokenSource linkedTokenSource)
        {
            IsCompleted = true;

            if (IsCanceled) return;
            
            if (OnFinal is not null)
                UnityMainThreadDispatcher.Enqueue(OnFinal);
            if (Continuation is not null)
                UnityMainThreadDispatcher.Enqueue(Continuation);
        }

        internal Task<T> AsTask() => TaskCompletionSource.Task;
        public static implicit operator Promise(Promise<T> promise) => promise.AsTask();
        public static implicit operator Promise<T>(Task<T> task) => new(task);
        public static implicit operator Task<T>(Promise<T> promise) => promise.AsTask();
    }
}
