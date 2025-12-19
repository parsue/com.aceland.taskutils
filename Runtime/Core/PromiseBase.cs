using System;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.Handles;

namespace AceLand.TaskUtils.Core
{
    public class PromiseBase<T> : Awaiter<T>
    {
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            Cancel();
        }

        public override void Cancel()
        {
            base.Cancel();
            CatchHandle.Dispose();
            TokenSource?.Cancel();
            TokenSource?.Dispose();
        }
        
        internal IPromiseDispatcher Dispatcher => Promise.Dispatcher;
        internal CatchHandle CatchHandle { get; } = new();

        protected Action OnFinal { get; set; }
        
        protected CancellationTokenSource TokenSource;
        
        protected void OnException(Task t)
        {
            if (t.Exception?.InnerExceptions.Count > 0)
            {
                foreach (var exception in t.Exception.InnerExceptions)
                    CatchHandle.Invoke(exception);

                Exception = t.Exception.InnerExceptions[0]; 
            }
            else
            {
                Exception = t.Exception ?? new Exception("unknown exception");
                CatchHandle.Invoke(Exception);
            }
        }

        protected void OnFinalize(CancellationTokenSource linkedTokenSource)
        {
            IsCompleted = true;
            if (Disposed) return;

            Promise.Dispatcher.Run(OnFinal);
            Promise.Dispatcher.Run(Continuation);
        }
    }
}