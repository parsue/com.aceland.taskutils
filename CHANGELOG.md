# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

---

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
