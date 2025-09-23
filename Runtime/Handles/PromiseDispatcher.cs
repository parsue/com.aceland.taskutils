using System;
using System.Collections;
using System.Threading.Tasks;
using AceLand.PlayerLoopHack;
using AceLand.TaskUtils.Mono;
using AceLand.TaskUtils.PlayerLoopSystems;
using UnityEngine;

namespace AceLand.TaskUtils.Handles
{
    internal class PromiseDispatcher : IPromiseDispatcher
    {
        internal bool Ready;
        internal PromiseAgent PromiseAgent;

        public void Run(Action action, PlayerLoopState state = PlayerLoopState.Initialization)
        {
            UnityMainThreadDispatchers.Enqueue(action, state);
        }

        public void RunOnEndOfFrame(Action action)
        {
            if (action == null) return;
            StartCoroutine(EndOfFrameCoroutine(action));
        }

        public void StartCoroutine(IEnumerator enumerator)
        {
            if (!Ready)
            {
                Promise.WaitUntil(() => Ready)
                    .Then(() => RunCoroutine(enumerator));

                return;
            }

            RunCoroutine(enumerator);
        }

        public Promise StartCoroutineAsTask(IEnumerator enumerator)
        {
            return RunCoroutineAsTask(enumerator);
        }

        private static IEnumerator EndOfFrameCoroutine(Action action)
        {
            yield return new WaitForEndOfFrame();
            action?.Invoke();
        }

        private void RunCoroutine(IEnumerator enumerator)
        {
            if (!PromiseAgent || PromiseAgent.Destroyed)
                throw new UnityException("Promise Agent Destroyed");
            
            PromiseAgent.StartCoroutine(enumerator);
        }

        private Task RunCoroutineAsTask(IEnumerator enumerator)
        {
            if (!PromiseAgent || PromiseAgent.Destroyed)
                throw new UnityException("Promise Agent Destroyed");

            var tcs = new TaskCompletionSource<object>();
            StartCoroutine(CoroutineToTask(enumerator, tcs));
            return tcs.Task;
        }
        
        private static IEnumerator CoroutineToTask(IEnumerator enumerator, TaskCompletionSource<object> tcs)
        {
            while (true)
            {
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        tcs.TrySetResult(null);
                        yield break;
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                    yield break;
                }
                yield return enumerator.Current;
            }
        }
    }
}