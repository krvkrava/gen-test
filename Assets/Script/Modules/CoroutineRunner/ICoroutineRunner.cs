using System;
using System.Collections;
using System.Threading.Tasks;
using Strx.Expansions.Modules.ModulesManagement.Attributes;
using UnityEngine;

namespace Modules.CoroutineRunner
{
    [ModuleProvider]
    public interface ICoroutineRunner
    {
        Task RunAsync(IEnumerator enumerator);
        Task RunWithProgressAsync(IEnumerator enumerator, Func<float> getProgressFunc, Action<float> progressChanged);

        Task RunWithProgressAsync(AsyncOperation asyncOperation, Func<float> getProgressFunc,
            Action<float> progressChanged);

        void RunWithProgress(IEnumerator enumerator, Action callback, Func<float> getProgressFunc,
            Action<float> progressChanged);

        void Run(IEnumerator enumerator, Action callback);
    }
}