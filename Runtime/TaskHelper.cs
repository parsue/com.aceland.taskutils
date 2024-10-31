using System;
using System.Threading;
using AceLand.TaskUtils.PlayerLoopSystems;

namespace AceLand.TaskUtils
{
    public static class TaskHelper
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

        public static void AddApplicationQuitListener(Action listener) => 
            ApplicationAliveSystem.OnApplicationQuit += listener;
        public static void RemoveApplicationQuitListener(Action listener) => 
            ApplicationAliveSystem.OnApplicationQuit -= listener;
    }
}
