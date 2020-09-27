using System;
using UniRx;
using UnityEngine;

namespace MainLevel.TetrisElements.Input
{
    public class InputReceiver : MonoBehaviour
    {
        public IObservable<bool> HorizontalButtonClicked => _horizontalClickedSubject;
        public IObservable<Unit> RotateButtonClicked => _rotateClickedSubject;
        public IObservable<Unit> MoveDownButtonClicked => _moveDownClickedSubject;

        private Subject<bool> _horizontalClickedSubject = new Subject<bool>();
        private Subject<Unit> _rotateClickedSubject = new Subject<Unit>();
        private Subject<Unit> _moveDownClickedSubject = new Subject<Unit>();

        public static InputReceiver Create()
        {
            return new GameObject("InputReceiver").AddComponent<InputReceiver>();
        }
        
        private void Update()
        {
            CheckForInput();
        }

        private void CheckForInput()
        {
            if(UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
                _horizontalClickedSubject.OnNext(true);
            
            if(UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
                _horizontalClickedSubject.OnNext(false);
            
            if(UnityEngine.Input.GetKeyDown(KeyCode.Space))
                _rotateClickedSubject.OnNext(Unit.Default);
            
            if(UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
                _moveDownClickedSubject.OnNext(Unit.Default);
        }
    }
}