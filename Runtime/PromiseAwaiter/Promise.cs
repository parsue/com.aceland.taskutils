using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.PromiseAwaiter.Base;

namespace AceLand.TaskUtils.PromiseAwaiter
{
    public sealed class Promise : Awaiter<bool>
    {
        internal Promise(Task task)
        {
            HandleTask(task);
        }
        
        internal Promise(Action action, Task task, bool isFinal)
        {
            if (isFinal) Final(action);
            else Then(action);
            HandleTask(task);
        }
        
        internal Promise(Func<Task> action, Task task)
        {
            Then(action);
            HandleTask(task);
        }

        internal Promise(Action<Exception> action, Task task)
        {
            Catch(action);
            HandleTask(task);
        }
        
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            OnSuccess = null;
            OnError = null;
            OnFinal = null;
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }
        
        private Action OnSuccess { get; set; }
        private Func<Task> OnSuccessTask { get; set; }
        private Action<Exception> OnError { get; set; }
        private Action OnFinal { get; set; }
        public override bool GetResult() => IsCompleted;
        
        private CancellationTokenSource _tokenSource;

        public Promise Then(Action onSuccess)
        {
            if (Disposed || IsCompleted) return this;
            OnSuccess += onSuccess;
            return this;
        }
        
        public Promise Then(Func<Task> onSuccess)
        {
            if (Disposed || IsCompleted) return this;
            OnSuccessTask += onSuccess;
            return this;
        }
        
        public Promise Catch(Action<Exception> onError)
        {
            if (Disposed || IsCompleted) return this;
            OnError += onError;
            return this;
        }
        
        public Promise Final(Action onFinal)
        {
            if (Disposed || IsCompleted) return this;
            OnFinal += onFinal;
            return this;
        }

        private void HandleTask(Task task)
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
                        
                        OnSuccess?.Invoke();
                        
                        if (OnSuccessTask is not null)
                            await OnSuccessTask();
                        
                        TaskCompletionSource.TrySetResult(true);
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

        public static Promise WhenAll(Promise[] promises) =>
            Task.WhenAll(promises.Select(promise => promise.AsTask()).ToArray());
        public static Promise<T[]> WhenAll<T>(Promise<T>[] promises) =>
            Task.WhenAll(promises.Select(p => p.AsTask()).ToArray());

        internal Task AsTask() => TaskCompletionSource.Task;
        public static implicit operator Promise(Task task) => new(task);
        public static implicit operator Task(Promise promise) => promise.AsTask();
    }
}
