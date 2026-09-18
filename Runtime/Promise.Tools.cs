#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using AceLand.Lifecycle;

namespace AceLand.TaskUtils
{
    public sealed partial class Promise
    {
        public async Task WaitForSeconds(float seconds, CancellationToken? token)
        {
            var tk = token ?? LifecycleToken.ApplicationAlive;
            var ms = (int)(seconds * 1000);
            await Task.Delay(ms, tk);
        }

        public async Task WaitForScaledTime(float seconds, CancellationToken? token = null)
        {
            var tk = token ?? LifecycleToken.ApplicationAlive;
            await LifecycleFrame.RunDelayed(() => {}, seconds, false, tk);
        }

        public async Task WaitUntil(Func<bool> condition, CancellationToken? token = null)
        {
            var tk = token ?? LifecycleToken.ApplicationAlive;
            while (!tk.IsCancellationRequested || !condition())
                await Task.Delay(100, tk);
        }

        public async Task WaitForNextFrame(CancellationToken? token = null)
        {
            var tk = token ?? LifecycleToken.ApplicationAlive;
            await LifecycleFrame.RunNextFrame(() => {}, PlayerLoopPoint.EarlyUpdate, tk);
        }

        public async Task WaitForNextFixedFrame(CancellationToken? token = null)
        {
            var tk = token ?? LifecycleToken.ApplicationAlive;
            await LifecycleFrame.RunNextFrame(() => {}, PlayerLoopPoint.FixedUpdate, tk);
        }

        public async Task WaitForFrames(int frames, CancellationToken? token = null)
        {
            var tk = token ?? LifecycleToken.ApplicationAlive;
            if (frames <= 0) return;
            await LifecycleFrame.RunAfterFrames(() => {}, frames, tk);
        }
    }
}