using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AceLand.Library.Disposable;

namespace AceLand.TaskUtils.Core
{
    public abstract class Awaiter<T> : DisposableObject, INotifyCompletion
    {
        internal Awaiter() => TaskCompletionSource = new TaskCompletionSource<T>();

        ~Awaiter() => Dispose(false);
        
        public virtual Awaiter<T> GetAwaiter() => this;
        public virtual T GetResult() => Result;
        
        public virtual bool IsCompleted { get; protected set; }
        public virtual bool IsSuccess { get; protected set; }
        public virtual bool IsFault { get; protected set; }
        public virtual bool IsCanceled { get; protected set; }
        public virtual Exception Exception { get; protected set; }
        public virtual T Result { get; protected set; }

        internal readonly TaskCompletionSource<T> TaskCompletionSource;
        private protected Action Continuation;

        protected override void DisposeManagedResources()
        {
            Cancel();
        }

        protected virtual void Success()
        {
            IsSuccess = true;
            try { TaskCompletionSource?.SetResult(Result); }
            catch { }
        }

        protected virtual void Fault()
        {
            IsFault = true;
            try { TaskCompletionSource?.SetException(Exception); }
            catch { }
        }

        public virtual void Cancel()
        {
            IsCanceled = true;
            try { TaskCompletionSource?.SetCanceled(); }
            catch { }
        }
        
        public virtual void OnCompleted(Action continuation)
        {
            if (Disposed) return;
            
            if (IsCompleted)
            {
                continuation.Invoke();
                return;
            }

            Continuation += continuation;
        }
    }
}