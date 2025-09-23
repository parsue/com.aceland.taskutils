using System;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.Core;

namespace AceLand.TaskUtils
{
    public sealed partial class Promise : PromiseBase<bool>
    {
        internal static Promise Create<TException>(Task task,
            Action thenAction = null,
            Func<Task> thenTask = null,
            Action<TException> catchAction = null,
            Action finalAction = null)
            where TException : Exception
        {
            var p = new Promise();
            
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
        
        private Action OnSuccess { get; set; }
        private Func<Task> OnSuccessTask { get; set; }

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
                var e = CatchHandle.GetException<Exception>();
                CatchHandle.Invoke(e);
                return this;
            }

            CatchHandle.AddHandler(onError);
            return this;
        }
        
        public Promise Catch<T>(Action<T> onError)
            where T : Exception
        {
            if (Disposed || IsCanceled) return this;
            
            if (IsFault)
            {
                var eT = CatchHandle.GetException<T>();
                if (eT == null) return Catch(onError);
                
                CatchHandle.Invoke(Exception);
                return this;
            }

            CatchHandle.AddHandler(onError);
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
            TokenSource = new CancellationTokenSource();
            var linkedToken = LinkedOrApplicationAliveToken(TokenSource,
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
                            Dispatcher.Run(OnSuccess);
                            Success();
                        }
                    }

                    OnFinalize(linkedTokenSource);
                },
                cancellationToken: linkedToken
            );
        }

        public static implicit operator Promise(Task task) => Create<Exception>(task);
        public static implicit operator Task(Promise promise) => promise.AsTask();
    }
}
