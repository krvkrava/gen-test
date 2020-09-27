using UnityEngine;

namespace MainLevel.UI
{
    public class PopupController : MonoBehaviour
    {
        [SerializeField] private GameObject _gameOverPopup;

        public void ShowGameOverPopup()
        {
            _gameOverPopup.gameObject.SetActive(transform);
        }
    }
}