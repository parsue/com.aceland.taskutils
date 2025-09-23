using System;
using System.Collections;
using AceLand.PlayerLoopHack;

namespace AceLand.TaskUtils
{
    public interface IPromiseDispatcher
    {
        void Run(Action action, PlayerLoopState state = PlayerLoopState.Initialization);
        void RunOnEndOfFrame(Action action);
        void StartCoroutine(IEnumerator enumerator);
        Promise StartCoroutineAsTask(IEnumerator enumerator);
    }
}