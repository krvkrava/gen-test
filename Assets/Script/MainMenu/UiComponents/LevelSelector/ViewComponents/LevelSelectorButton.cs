using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.UiElements.LevelSelector
{
    public class LevelSelectorButton : MonoBehaviour
    {
        public IObservable<Unit> OnLevelButtonClicked => _levelButtonclickedSubject;

        [SerializeField] private Button _button;
        [SerializeField] private Text _text;

        private Subject<Unit> _levelButtonclickedSubject = new Subject<Unit>();

        public void Initialize(bool isActive, string text)
        {
            gameObject.SetActive(isActive);
            _text.text = text;
            _button.onClick.AddListener(() => _levelButtonclickedSubject.OnNext(Unit.Default));
        }
    }
}