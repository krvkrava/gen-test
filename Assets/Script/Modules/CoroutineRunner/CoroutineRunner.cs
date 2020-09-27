using System;
using System.Collections;
using System.Threading.Tasks;
using Strx.Expansions.Modules.ModulesManagement;
using AsyncOperation = UnityEngine.AsyncOperation;

namespace Modules.CoroutineRunner
{
    public class CoroutineRunner : StrxModule, ICoroutineRunnerSchema, ICoroutineRunner
    {
        private CoroutineRunnerAgent _agent;
        
        public static CoroutineRunner Create()
        {
            var runner = new CoroutineRunner();
            runner._agent = CoroutineRunnerAgent.Create();

            return runner;
        }

        public Task RunAsync(IEnumerator enumerator)
        {    
            var tcs = new TaskCompletionSource<object>();
            _agent.Run(enumerator, () => tcs.SetResult(null));

            return tcs.Task;
        }
        
        public Task RunWithProgressAsync(IEnumerator enumerator, 
            Func<float> getProgressFunc, Action<float> progressChanged)
        {
            var tcs = new TaskCompletionSource<object>();
            _agent.RunWithProgress(enumerator, () => tcs.SetResult(null), 
                getProgressFunc, progressChanged);

            return tcs.Task;
        }
        
        public Task RunWithProgressAsync(AsyncOperation asyncOperation, 
            Func<float> getProgressFunc, Action<float> progressChanged)
        {
            var tcs = new TaskCompletionSource<object>();
            _agent.RunWithProgress(asyncOperation, () => tcs.SetResult(null), 
                getProgressFunc, progressChanged);

            return tcs.Task;
        }

        public void RunWithProgress(IEnumerator enumerator, Action callback, Func<float> getProgressFunc, Action<float> progressChanged)
        {
            _agent.RunWithProgress(enumerator, callback, getProgressFunc, progressChanged);
        }

        public void Run(IEnumerator enumerator, Action callback)
        {
            _agent.Run(enumerator, callback);
        }

        public void Dispose()
        {
            _agent.Dispose();
        }
    }
}