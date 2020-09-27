using System;
using System.Collections;

namespace Modules.CoroutineRunner
{
    public interface ICoroutineRunnerSchema : IDisposable
    {
        void RunWithProgress(IEnumerator enumerator, Action callback,
            Func<float> getProgressFunc, Action<float> progressChanged);

        void Run(IEnumerator enumerator, Action callback);
    }
}