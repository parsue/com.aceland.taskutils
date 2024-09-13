using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AceLand.Library.Disposable;
using Cysharp.Threading.Tasks;

namespace AceLand.TaskUtils.PromiseAwaiter
{
    public sealed class Promise : DisposableObject, INotifyCompletion
    {
        private Promise(UniTask task)
        {
            HandleTask(task);
        }
        
        internal Promise(Action action, UniTask task, bool isFinal)
        {
            if (isFinal) Final(action);
            else Then(action);
            HandleTask(task);
        }
        
        internal Promise(Func<Task> action, UniTask task)
        {
            Then(action);
            HandleTask(task);
        }

        internal Promise(Action<Exception> action, UniTask task)
        {
            Catch(action);
            HandleTask(task);
        }
        
        protected override void DisposeManagedResources()
        {
            OnSuccess = null;
            OnError = null;
            OnFinal = null;
        }
        
        private Action OnSuccess { get; set; }
        private Func<Task> OnSuccessTask { get; set; }
        private Action<Exception> OnError { get; set; }
        private Action OnFinal { get; set; }
        public bool IsCompleted { get; private set; }
        public bool GetResult() => IsCompleted;
        public Promise GetAwaiter() => this;

        private TaskCompletionSource<bool> _taskCompletionSource;

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

        public void OnCompleted(Action continuation)
        {
            if (Disposed) return;

            if (IsCompleted)
            {
                continuation.Invoke();
                return;
            }

            Final(continuation);
        }

        private void HandleTask(UniTask task)
        {
            _taskCompletionSource = new TaskCompletionSource<bool>();
            
            UniTask.RunOnThreadPool(async () =>
                {
                    await UniTask.Yield();
                    try
                    {
                        await task;
                        OnSuccess?.Invoke();
                        
                        if (OnSuccessTask is not null)
                            await OnSuccessTask();
                        
                        _taskCompletionSource.TrySetResult(true);
                    }
                    catch (Exception e)
                    {
                        OnError?.Invoke(e);
                        _taskCompletionSource.SetException(e);
                    }
                    finally
                    {
                        OnFinal?.Invoke();
                        IsCompleted = true;
                    }
                },
                cancellationToken: TaskHandler.ApplicationAliveToken,
                configureAwait: false
            );
        }

        public static implicit operator Promise(Task task) => new(task.AsUniTask());
        public static implicit operator Promise(UniTask task) => new(task);
        public static implicit operator Task(Promise promise) => promise._taskCompletionSource.Task;
        public static implicit operator UniTask(Promise promise) => promise._taskCompletionSource.Task.AsUniTask();
    }
}
