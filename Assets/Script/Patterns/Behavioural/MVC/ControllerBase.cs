using System.Threading.Tasks;
using Smooth.Algebraics;
using Smooth.Algebraics.Results;
using Strx.Expansions.Extensions.Algebraic;
using Strx.Expansions.Extensions.Primitive;

namespace Patterns.MVC
{
    public abstract class ControllerBase<M, V> where M : ModelBase where V : ViewBase<M>
    {
        protected V View { get; private set; }
        protected M Model { get; private set; }

        protected static Task<Result<C>> Create<C>(bool initialize = true) where C : ControllerBase<M, V>, new()
        {
            var controller = new C();
            return !initialize
                ? controller.ToValue().ToTask()
                : controller.InitAsync()
                .ThenContextAsync(controller, (_, _controller) => _controller);
        }

        protected abstract Task<Result<Unit>> SetInitialAsync();
        protected abstract Task<Result<M>> LoadModelAsync();
        protected abstract Task<Result<V>> LoadViewAsync();

        protected Task<Result<Unit>> InitAsync()
            => SetInitialAsync().SpecifyAsync(() => $"Cannot initialize controller of type: {GetType().Name}")
                .ThenContextAsync(_ => LoadModelAsync()
                    .SpecifyAsync($"Cannot create model of type: {typeof(M).Name}")
                )
                .ThenContextAsync(this, (model, _this) =>
                {
                    _this.Model = model;
                    return _this.LoadViewAsync().SpecifyAsync(() => $"Cannot create view of type: {typeof(V).Name}")
                        .ThenContextAsync(model, async (view, _model) =>
                        {
                            var viewInitResult = await view.InitModel(_model);
                            return viewInitResult.IsError
                                        ? viewInitResult.ConvertErrorTo<V>($"Unable to init module {typeof(V).Name}")
                                            : view.ToValue();
                            
                        });
                })
                .ThenContextAsync(this, (view, _this) => _this.View = view)
                .ToUnitAsync();
    }
}