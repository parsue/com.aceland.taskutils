using System;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.PromiseAwaiter.Core;

namespace AceLand.TaskUtils.PromiseAwaiter
{
    public sealed class Promise<T> : Awaiter<T>
    {
        internal Promise(Task<T> task)
        {
            HandleTask(task);
        }
        
        internal Promise(Action<T> action, Task<T> task)
        {
            Then(action);
            HandleTask(task);
        }
        
        internal Promise(Func<T, Task> action, Task<T> task)
        {
            Then(action);
            HandleTask(task);
        }

        internal Promise(Action<Exception> action, Task<T> task)
        {
            Catch(action);
            HandleTask(task);
        }

        internal Promise(Action action, Task<T> task)
        {
            Final(action);
            HandleTask(task);
        }
        
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            Cancel();
        }

        public override void Cancel()
        {
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
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                _tokenSource.Token,
                TaskHandler.ApplicationAliveToken
            );
            
            Task.Run(async () =>
                {
                    await Task.Yield();
                    try
                    {
                        if (task.Status is not TaskStatus.Running)
                            task.Start();
                        
                        while (!task.IsCompleted)
                        {
                            linkedCts.Token.ThrowIfCancellationRequested();
                            Thread.Yield();
                        }
                        
                        Result = task.Result;
                        OnSuccess?.Invoke(Result);
                        
                        if (OnSuccessTask is not null)
                            await OnSuccessTask(Result);
                        
                        TaskCompletionSource.TrySetResult(Result);
                    }
                    catch (Exception e)
                    {
                        OnError?.Invoke(e);
                        TaskCompletionSource.TrySetException(e);
                    }
                    finally
                    {
                        IsCompleted = true;
                        OnFinal?.Invoke();
                        Continuation?.Invoke();
                    }
                },
                linkedCts.Token
            );
        }

        internal Task<T> AsTask() => TaskCompletionSource.Task;
        public static implicit operator Promise(Promise<T> promise) => promise.AsTask();
        public static implicit operator Promise<T>(Task<T> task) => new(task);
        public static implicit operator Task<T>(Promise<T> promise) => promise.AsTask();
    }
}
