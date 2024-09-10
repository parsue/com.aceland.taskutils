using System;
using System.Runtime.CompilerServices;
using AMVR.LocalTools.Disposable;
using Cysharp.Threading.Tasks;

namespace AceLand.TasksUtils.PromiseAwaiter
{
    public sealed class Promise<T> : DisposableObject, INotifyCompletion
    {
        private Promise(UniTask<T> task)
        {
            HandleTask(task);
        }
        
        internal Promise(Action<T> action, UniTask<T> task)
        {
            Then(action);
            HandleTask(task);
        }

        internal Promise(Action<Exception> action, UniTask<T> task)
        {
            Catch(action);
            HandleTask(task);
        }

        internal Promise(Action action, UniTask<T> task)
        {
            Final(action);
            HandleTask(task);
        }
        
        protected override void DisposeManagedResources()
        {
            Result = default;
            OnSuccess = null;
            OnError = null;
            OnFinal = null;
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

        public T GetResult() => Result;
        public Promise<T> GetAwaiter() => this;
        
        private Action<T> OnSuccess { get; set; }
        private Action<Exception> OnError { get; set; }
        private Action OnFinal { get; set; }
        public bool IsCompleted { get; private set; }
        public T Result { get; private set; }

        public Promise<T> Then(Action<T> onSuccess)
        {
            if (Disposed || IsCompleted) return this;
            OnSuccess += onSuccess;
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

        private void HandleTask(UniTask<T> task)
        {
            UniTask.RunOnThreadPool(async () =>
                {
                    await UniTask.Yield();
                    try
                    {
                        Result = await task;
                        OnSuccess?.Invoke(Result);
                    }
                    catch (Exception e)
                    {
                        OnError?.Invoke(e);
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

        public static implicit operator Promise<T>(Task<T> task) => new(task.AsUniTask());
        public static implicit operator Promise<T>(UniTask<T> task) => new(task);
    }
}
