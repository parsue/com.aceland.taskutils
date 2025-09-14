using System;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.Core;
using AceLand.TaskUtils.PlayerLoopSystems;

namespace AceLand.TaskUtils
{
    public sealed class Promise<T> : PromiseBase<T>
    {
        internal static Promise<T> Create<TException>(Task<T> task,
            Action<T> thenAction = null,
            Func<T, Task> thenTask = null,
            Action<TException> catchAction = null, 
            Action finalAction = null)
            where TException : Exception
        {
            var p = new Promise<T>();
            
            if (thenAction is not null) p.Then(thenAction);
            if (thenTask is not null) p.Then(thenTask);
            if (catchAction is not null) p.Catch(catchAction);
            if (finalAction is not null) p.Final(finalAction);
            p.HandleTask(task);
            return p;
        }

        public override void Cancel()
        {
            base.Cancel();
            OnSuccess = null;
            OnSuccessTask = null;
        }
        
        private Action<T> OnSuccess { get; set; }
        private Func<T, Task> OnSuccessTask { get; set; }
        
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
                CatchHandle.Invoke(Exception);
                return this;
            }
            
            CatchHandle.AddHandler(onError);
            return this;
        }
        
        public Promise<T> Catch<TException>(Action<TException> onError)
            where TException : Exception
        {
            if (IsCanceled || Disposed) return this;
            
            if (IsFault)
            {
                CatchHandle.Invoke(Exception);
                return this;
            }
            
            CatchHandle.AddHandler(onError);
            return this;
        }
        
        public Promise<T> Final(Action onFinal)
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

        private void HandleTask(Task<T> task)
        {
            TokenSource = new CancellationTokenSource();
            var linkedToken = Promise.LinkedOrApplicationAliveToken(TokenSource,
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
                                UnityMainThreadDispatchers.Enqueue(() => OnSuccess(Result));
                            
                            Success();
                        }
                    }

                    OnFinalize(linkedTokenSource);
                },
                cancellationToken: linkedToken
            );
        }

        public static implicit operator Promise(Promise<T> promise) => promise.AsTask();
        public static implicit operator Promise<T>(Task<T> task) => Create<Exception>(task);
        public static implicit operator Task<T>(Promise<T> promise) => promise.AsTask();
    }
}
