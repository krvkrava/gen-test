using System;
using System.Threading.Tasks;
using Patterns.MVC;
using Smooth.Algebraics.Results;
using Smooth.Slinq;
using UniRx;
using UnityEngine;
using Unit = Smooth.Algebraics.Unit;

namespace MainMenu.UiElements.LevelSelector
{
    public class LevelSelectionView : ViewBase<LevelSelectionModel>
    {
        public IObservable<string> OnLevelSelected => _levelSelectedSubject;

        [SerializeField] private LevelSelectorButton[] _levelSelectionButtons;

        private Subject<string> _levelSelectedSubject = new Subject<string>(); 

        protected override Task<Result<Unit>> InitView()
        {
            Model.LevelInfos.SlinqWithIndex()
                .ForEach(this, (infoAndIndex, _this) =>
                {
                    var sceneName = infoAndIndex.Item1.DisplayName;
                    
                    var button = _this._levelSelectionButtons[infoAndIndex.Item2];
                    button.Initialize(infoAndIndex.Item1.Enabled, sceneName);
                    button.OnLevelButtonClicked.Subscribe(_ => _levelSelectedSubject.OnNext(sceneName))
                        .AddTo(this);
                });

            return Result.FromUnitValueAsync;
        }
    }
}