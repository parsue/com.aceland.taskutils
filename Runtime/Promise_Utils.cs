using System;
using System.Threading;
using System.Threading.Tasks;
using AceLand.TaskUtils.PlayerLoopSystems;

namespace AceLand.TaskUtils
{
    public sealed partial class Promise
    {
        public static CancellationToken ApplicationAliveToken => 
            ApplicationAliveSystem.ApplicationAliveTokenSource.Token;

        public static CancellationToken LinkedOrApplicationAliveToken(CancellationTokenSource tokenSource,
            out CancellationTokenSource linkedTokenSource)
        {
            if (tokenSource == null)
            {
                linkedTokenSource = null;
                return ApplicationAliveToken;
            }
            
            linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                tokenSource.Token,
                ApplicationAliveToken
            );
            return linkedTokenSource.Token;
        }

        public static Task WaitForSeconds(float seconds) =>
            Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(seconds), ApplicationAliveToken);
                },
                ApplicationAliveToken
            );

        public static Task WaitUntil(Func<bool> condition) =>
            Task.Run(async () =>
                {
                    while (!condition() && !ApplicationAliveToken.IsCancellationRequested)
                        await Task.Delay(100, ApplicationAliveToken);
                },
                ApplicationAliveToken
            );

        public static void EnqueueToDispatcher(Action action) =>
            UnityMainThreadDispatcher.Enqueue(action);
        public static void AddApplicationQuitListener(Action listener) => 
            ApplicationAliveSystem.OnApplicationQuit += listener;
        public static void RemoveApplicationQuitListener(Action listener) => 
            ApplicationAliveSystem.OnApplicationQuit -= listener;
    }
}