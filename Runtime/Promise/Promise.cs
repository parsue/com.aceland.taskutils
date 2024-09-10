using System;
using System.Runtime.CompilerServices;
using AMVR.LocalTools.Disposable;
using Cysharp.Threading.Tasks;

namespace AceLand.TasksUtils.PromiseAwaiter
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

        public Promise GetAwaiter() => this;
        
        private Action OnSuccess { get; set; }
        private Action<Exception> OnError { get; set; }
        private Action OnFinal { get; set; }
        public bool IsCompleted { get; private set; }

        public Promise Then(Action onSuccess)
        {
            if (Disposed || IsCompleted) return this;
            OnSuccess += onSuccess;
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

        private void HandleTask(UniTask task)
        {
            UniTask.RunOnThreadPool(async () =>
                {
                    await UniTask.Yield();
                    try
                    {
                        await task;
                        OnSuccess?.Invoke();
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
        
        public static implicit operator Promise(Task task) => new(task.AsUniTask());
        public static implicit operator Promise(UniTask task) => new(task);
    }
}
