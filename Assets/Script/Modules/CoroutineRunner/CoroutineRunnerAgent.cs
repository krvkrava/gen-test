using System;
using System.Collections;
using UnityEngine;

namespace Modules.CoroutineRunner
{
    public class CoroutineRunnerAgent : MonoBehaviour, ICoroutineRunnerSchema
    {
        public static CoroutineRunnerAgent Create()
        {
            var gameObject = new GameObject(nameof(CoroutineRunner));
            var runnerAgent = gameObject.AddComponent<CoroutineRunnerAgent>();
            
            DontDestroyOnLoad(gameObject);
            
            return runnerAgent;
        }
        
        public void Run(IEnumerator enumerator, Action callback)
        {
            StartCoroutine(RunCoroutineInternal(enumerator, callback));
        }

        public void RunWithProgress(IEnumerator enumerator, Action callback, 
            Func<float> getProgressFunc, Action<float> progressChanged)
        {
            StartCoroutine(RunEnumerableInternalWithProgress(enumerator, callback, getProgressFunc, progressChanged));
        }
        
        public void RunWithProgress(AsyncOperation asyncOperation, Action callback, 
            Func<float> getProgressFunc, Action<float> progressChanged)
        {   
            StartCoroutine(RunAsyncOperationInternalWithProgress(asyncOperation, callback, progressChanged));
        }

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }
        
        private IEnumerator RunEnumerableInternalWithProgress(IEnumerator enumerator, 
            Action callback, Func<float> getProgressFunc, Action<float> progressChanged)
        {
            var lastProgress = 0f;
            
            while (enumerator.MoveNext())
            {
                yield return null;

                var currentProgress = getProgressFunc();
                Mathf.Clamp01(currentProgress);
                
                if (currentProgress != lastProgress)
                {
                    lastProgress = currentProgress;
                    progressChanged(lastProgress);
                }
            }

            lastProgress = 1f;
            progressChanged(lastProgress);

            callback();
        }
        
        private IEnumerator RunAsyncOperationInternalWithProgress(AsyncOperation asyncOperation, 
            Action callback, Action<float> progressChanged)
        {
            var lastProgress = 0f;
            
            while (!asyncOperation.isDone)
            {
                yield return null;

                var currentProgress = asyncOperation.progress;
                Mathf.Clamp01(currentProgress);
                
                if (currentProgress != lastProgress)
                {
                    lastProgress = currentProgress;
                    progressChanged(lastProgress);
                }
            }

            lastProgress = 1f;
            progressChanged(lastProgress);

            callback();
        }
        
        private IEnumerator RunCoroutineInternal(IEnumerator enumerator, Action callback)
        {
            yield return enumerator;
            callback?.Invoke();
        }
    }
}