using System;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.PromiseAwaiter.Core;

namespace AceLand.TaskUtils.PromiseAwaiter
{
    public sealed class Promise<T> : Awaiter<T>
    {
        internal Promise(Task<T> task, Action<T> thenAction = null, Func<T, Task> thenTask = null, Action<Exception> catchAction = null, Action finalAction = null)
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
            if (Disposed || IsCompleted) return this;
            OnSuccess += onSuccess;
            return this;
        }
        
        public Promise<T> Then(Func<T, Task> onSuccess)
        {
            if (Disposed || IsCompleted) return this;
            OnSuccessTask += onSuccess;
            return this;
        }
        
        public Promise<T> Catch(Action<Exception> onError)
        {
            if (Disposed || IsCompleted) return this;
            OnError += onError;
            return this;
        }
        
        public Promise<T> Final(Action onFinal)
        {
            if (Disposed || IsCompleted) return this;
            OnFinal += onFinal;
            return this;
        }

        private void HandleTask(Task<T> task)
        {
            _tokenSource = new CancellationTokenSource();
            var linkedToken = TaskHandler.LinkedOrApplicationAliveToken(_tokenSource,
                out var linkedTokenSource);

            if (task.Status < TaskStatus.WaitingForActivation)
                task.Start();

            Task.Run(() =>
                {
                    while (!task.IsCompleted)
                        Thread.Yield();

                    if (task.IsCanceled)
                    {
                        TaskCompletionSource.TrySetCanceled();
                    }
                    else if (task.IsFaulted && task.Exception?.InnerExceptions.Count > 0)
                    {
                        foreach (var exception in task.Exception.InnerExceptions)
                            OnError?.Invoke(exception);
                        
                        TaskCompletionSource.TrySetException(task.Exception.InnerExceptions[0]);
                    }
                    else if (task.IsCompletedSuccessfully)
                    {
                        Result = task.Result;
                        OnSuccess?.Invoke(Result);
                        if (OnSuccessTask is not null)
                        {
                            var successTasks = OnSuccessTask(Result);
                            
                            while (!successTasks.IsCompleted)
                                Thread.Yield();
                        }
                        
                        TaskCompletionSource.TrySetResult(Result);
                    }

                    IsCompleted = true;
                    OnFinal?.Invoke();
                    Continuation?.Invoke();
                    linkedTokenSource?.Dispose();
                },
                linkedToken
            );
        }

        internal Task<T> AsTask() => TaskCompletionSource.Task;
        public static implicit operator Promise(Promise<T> promise) => promise.AsTask();
        public static implicit operator Promise<T>(Task<T> task) => new(task);
        public static implicit operator Task<T>(Promise<T> promise) => promise.AsTask();
    }
}
