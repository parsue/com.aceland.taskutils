using System;
using System.Collections;
using System.Threading.Tasks;
using AceLand.Library.Mono;
using UnityEngine;

namespace AceLand.TaskUtils.Mono
{
    public class PromiseAgent : Singleton<PromiseAgent>
    {
        private static bool Ready;
        private static bool Destroyed;

        private void OnEnable()
        {
            Ready = true;
            Destroyed = false;
        }

        private void OnDisable()
        {
            Ready = false;
            Debug.LogWarning("Promise Agent Disabling");
        }

        private void OnDestroy()
        {
            Destroyed = true;
            Debug.LogWarning("Promise Agent Destroyed");
        }

        public static void CoroutineAgent(IEnumerator enumerator)
        {
            if (Destroyed)
                throw new UnityException("Promise Agent Destroyed");
            
            if (!Ready)
            {
                Promise.WaitUntil(() => Ready)
                    .Then(() => Instance.StartCoroutine(enumerator));
                return;
            }

            Instance.StartCoroutine(enumerator);
        }

        internal static Task RunCoroutine(IEnumerator enumerator)
        {
            if (Destroyed)
                throw new UnityException("Promise Agent Destroyed");

            var tcs = new TaskCompletionSource<object>();
            CoroutineAgent(RunCoroutine(enumerator, tcs));
            return tcs.Task;
        }
        
        private static IEnumerator RunCoroutine(IEnumerator enumerator, TaskCompletionSource<object> tcs)
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