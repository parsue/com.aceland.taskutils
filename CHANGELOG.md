# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

---

## [1.0.9] - 2025-04-13
### Added
- [Dispatcher] for all PlayerLoopState
- [Utility] Promise.WaitForEndOfFrame(Action)
- [Extension] IEnumerator.RunCoroutine();
- [Extension] Action.EnqueueToDispatcher<T>(Action<T>, T, PlayerLoopState)
### Modified
- [Utility] Promise.EnqueueToDispatcher() can set PlayerLoopState now
- [Extension] Action.EnqueueToDispatcher can set PlayerLoopState now
### Removed
- [PromiseAgent] no access allowed, please use extension

## [1.0.8] - 2025-04-09
### Fixed
- [Singleton] PromiseAgent - not handle AsTask on Agent not yet Ready 
### Removed
- [Extension] IEnumerator<T>/IEnumerable/IENumerable<T> .AsTask()

## [1.0.7] - 2025-04-08
### Added
- [Extension] IEnumerator.AsTask()
- [Extension] IEnumerator<T>.AsTask()
- [Extension] IEnumerable.AsTask()
- [Extension] IEnumerable<T>.AsTask()
- [Singleton] PromiseAgent - instantiate on first since loaded and set as DontDestroyObject
- [Utility] Promise.WaitForSeconds(float)
- [Utility] Promise.WaitUntil(Func<bool>)

## [1.0.6] - 2025-02-04
### Fixed
- SafeRun without token issue

## [1.0.5] - 2025-01-25
### Added
- SafeRun Task for Coroutine:
  - Promise.SafeRun(Action action)
  - Promise.SafeRun(Func<Task> action)

## [1.0.4] - 2024-11-24
First public release. If you have an older version, please update or re-install.   
For detail please visit and bookmark our [GitBook](https://aceland-workshop.gitbook.io/aceland-unity-packages/)
