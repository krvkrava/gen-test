using System.Threading.Tasks;
using Smooth.Algebraics;
using Smooth.Algebraics.Results;
using UnityEngine;

namespace Patterns.MVC
{
    public abstract class ViewBase<M> : MonoBehaviour
    {
        protected M Model { get; private set; }

        public Task<Result<Unit>> InitModel(M model)
        {
            Model = model;
            return InitView();
        }

        protected abstract Task<Result<Unit>> InitView();
    }
}