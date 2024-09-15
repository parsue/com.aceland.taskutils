using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AceLand.Library.Disposable;

namespace AceLand.TaskUtils.PromiseAwaiter.Base
{
    public abstract class Awaiter<T> : DisposableObject, INotifyCompletion
    {
        internal Awaiter() => TaskCompletionSource = new TaskCompletionSource<T>();

        ~Awaiter() => Dispose(false);
        
        public virtual Awaiter<T> GetAwaiter() => this;
        public virtual T GetResult() => Result;
        
        protected virtual bool IsCompleted { get; set; }
        protected virtual T Result { get; set; }

        private protected readonly TaskCompletionSource<T> TaskCompletionSource;
        private protected Action Continuation;

        protected override void DisposeManagedResources()
        {
            Continuation = null;
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