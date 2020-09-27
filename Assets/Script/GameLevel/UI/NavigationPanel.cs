using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MainLevel.UI
{
    public class NavigationPanel : MonoBehaviour
    {
        public IObservable<Unit> OnBackButtonClicked => _onBackButtonClicked;
    
        [SerializeField] private Button _backButton;

        private Subject<Unit> _onBackButtonClicked = new Subject<Unit>();

        private void Awake()
        {
            _backButton.onClick.AddListener(() => _onBackButtonClicked.OnNext(Unit.Default));
        }
    }
}